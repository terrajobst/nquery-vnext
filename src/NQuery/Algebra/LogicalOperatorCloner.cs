#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Algebra
{
    // Produces a deep copy of a logical subtree with every value slot it *defines*
    // replaced by a fresh duplicate, and every reference remapped to match. Two clones
    // of the same subtree are therefore slot-disjoint, which is what lets an operator
    // consume an input twice -- the full-outer-join expansion needs this, and CTE
    // instantiation eventually will too.
    //
    // The discipline mirrors the bound-tree Instantiator: one slot map per clone;
    // children are cloned first, then the slots this node newly introduces are
    // duplicated, then every slot reference is remapped through the map. A reference to
    // a slot defined outside the subtree (an outer correlation) isn't in the map and is
    // left unchanged, so both clones still point at the same enclosing row.
    //
    // Only the operators that genuinely introduce slots duplicate them (table scan
    // columns, computed/aggregated outputs, unified outputs, a semi-join probe);
    // everything else -- grouping keys, sort/top keys, projections -- references slots a
    // descendant already remapped, so it goes through Map.
    internal sealed class LogicalOperatorCloner
    {
        private readonly Dictionary<ValueSlot, ValueSlot> _slotMap = new();

        public LogicalOperator Clone(LogicalOperator node)
        {
            switch (node.Kind)
            {
                case LogicalOperatorKind.Empty:
                case LogicalOperatorKind.Constant:
                    return node;
                case LogicalOperatorKind.TableScan:
                    return CloneTableScan((LogicalTableScan)node);
                case LogicalOperatorKind.Filter:
                    return CloneFilter((LogicalFilter)node);
                case LogicalOperatorKind.Compute:
                    return CloneCompute((LogicalCompute)node);
                case LogicalOperatorKind.Project:
                    return CloneProject((LogicalProject)node);
                case LogicalOperatorKind.Join:
                    return CloneJoin((LogicalJoin)node);
                case LogicalOperatorKind.Apply:
                    return CloneApply((LogicalApply)node);
                case LogicalOperatorKind.Aggregate:
                    return CloneAggregate((LogicalAggregate)node);
                case LogicalOperatorKind.Union:
                    return CloneUnion((LogicalUnion)node);
                case LogicalOperatorKind.IntersectOrExcept:
                    return CloneIntersectOrExcept((LogicalIntersectOrExcept)node);
                case LogicalOperatorKind.Sort:
                    return CloneSort((LogicalSort)node);
                case LogicalOperatorKind.Top:
                    return CloneTop((LogicalTop)node);
                case LogicalOperatorKind.Assert:
                    return CloneAssert((LogicalAssert)node);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        // A reference: the slot a descendant introduced (or an untouched outer slot).
        private ValueSlot Map(ValueSlot slot)
        {
            return _slotMap.TryGetValue(slot, out var mapped) ? mapped : slot;
        }

        // A definition: a fresh slot the first time this original slot is seen.
        private ValueSlot Introduce(ValueSlot slot)
        {
            if (_slotMap.TryGetValue(slot, out var existing))
                return existing;

            var fresh = slot.Duplicate();
            _slotMap.Add(slot, fresh);
            return fresh;
        }

        private LogicalOperator CloneTableScan(LogicalTableScan node)
        {
            var original = node.TableInstance;
            var factory = node.DefinedValues[0].ValueSlot.Factory;
            var instanceName = factory.CreateNamed(original.Name, typeof(int)).Name;
            var instance = new TableInstanceSymbol(instanceName, original.Table, factory);
            var byColumn = instance.ColumnInstances.ToDictionary(c => c.Column);

            foreach (var column in original.ColumnInstances)
                _slotMap[column.ValueSlot] = byColumn[column.Column].ValueSlot;

            var definedValues = node.DefinedValues.Select(d => byColumn[d.Column]).ToImmutableArray();
            return new LogicalTableScan(instance, definedValues);
        }

        private LogicalOperator CloneFilter(LogicalFilter node)
        {
            var input = Clone(node.Input);
            var conditions = node.Conditions.Select(CloneExpression).ToImmutableArray();
            return new LogicalFilter(input, conditions);
        }

        private LogicalOperator CloneCompute(LogicalCompute node)
        {
            var input = Clone(node.Input);
            var outputs = node.DefinedValues.Select(v => Introduce(v.ValueSlot)).ToImmutableArray();
            var definedValues = node.DefinedValues
                                    .Select((v, i) => new LogicalComputedValue(CloneExpression(v.Expression), outputs[i]))
                                    .ToImmutableArray();
            return new LogicalCompute(input, definedValues);
        }

        private LogicalOperator CloneProject(LogicalProject node)
        {
            var input = Clone(node.Input);
            var outputs = node.Outputs.Select(Map).ToImmutableArray();
            return new LogicalProject(input, outputs);
        }

        private LogicalOperator CloneJoin(LogicalJoin node)
        {
            var left = Clone(node.Left);
            var right = Clone(node.Right);
            var conditions = node.Conditions.Select(CloneExpression).ToImmutableArray();
            var passthru = node.PassthruPredicate is null ? null : CloneExpression(node.PassthruPredicate);
            var probe = node.Probe is null ? null : Introduce(node.Probe);
            return new LogicalJoin(node.JoinKind, left, right, conditions, probe, passthru);
        }

        private LogicalOperator CloneApply(LogicalApply node)
        {
            var left = Clone(node.Left);
            var right = Clone(node.Right);
            var probe = node.Probe is null ? null : Introduce(node.Probe);
            var passthru = node.Passthru is null ? null : CloneExpression(node.Passthru);
            return new LogicalApply(node.ApplyKind, left, right, probe, passthru);
        }

        private LogicalOperator CloneAggregate(LogicalAggregate node)
        {
            var input = Clone(node.Input);
            var groups = node.Groups.Select(g => new BoundComparedValue(Map(g.ValueSlot), g.Comparer)).ToImmutableArray();
            var aggregates = node.Aggregates
                                 .Select(a => new LogicalAggregatedValue(Introduce(a.Output), a.Aggregate, a.Aggregatable, CloneExpression(a.Argument)))
                                 .ToImmutableArray();
            return new LogicalAggregate(input, groups, aggregates);
        }

        private LogicalOperator CloneUnion(LogicalUnion node)
        {
            var inputs = node.Inputs.Select(Clone).ToImmutableArray();
            var definedValues = node.DefinedValues
                                    .Select(v => new BoundUnifiedValue(Introduce(v.ValueSlot), v.InputValueSlots.Select(Map)))
                                    .ToImmutableArray();
            return new LogicalUnion(node.IsUnionAll, inputs, definedValues, node.Comparers);
        }

        private LogicalOperator CloneIntersectOrExcept(LogicalIntersectOrExcept node)
        {
            // Its output reuses the left's slots, so it introduces nothing of its own.
            var left = Clone(node.Left);
            var right = Clone(node.Right);
            return new LogicalIntersectOrExcept(node.IsIntersect, left, right, node.Comparers);
        }

        private LogicalOperator CloneSort(LogicalSort node)
        {
            var input = Clone(node.Input);
            var sortedValues = node.SortedValues.Select(v => new BoundComparedValue(Map(v.ValueSlot), v.Comparer)).ToImmutableArray();
            return new LogicalSort(node.IsDistinct, input, sortedValues);
        }

        private LogicalOperator CloneTop(LogicalTop node)
        {
            var input = Clone(node.Input);
            var tieEntries = node.TieEntries.Select(v => new BoundComparedValue(Map(v.ValueSlot), v.Comparer)).ToImmutableArray();
            return new LogicalTop(input, node.Limit, tieEntries);
        }

        private LogicalOperator CloneAssert(LogicalAssert node)
        {
            var input = Clone(node.Input);
            return new LogicalAssert(input, CloneExpression(node.Condition), node.Message);
        }

        public LogicalExpression CloneExpression(LogicalExpression node)
        {
            switch (node.Kind)
            {
                case LogicalExpressionKind.Literal:
                case LogicalExpressionKind.Variable:
                    return node;
                case LogicalExpressionKind.ValueSlot:
                    var valueSlot = (LogicalValueSlotExpression)node;
                    var mapped = Map(valueSlot.ValueSlot);
                    return mapped == valueSlot.ValueSlot ? node : new LogicalValueSlotExpression(mapped);
                case LogicalExpressionKind.Unary:
                    var unary = (LogicalUnaryExpression)node;
                    return new LogicalUnaryExpression(unary.OperatorKind, unary.Result, CloneExpression(unary.Expression));
                case LogicalExpressionKind.Binary:
                    var binary = (LogicalBinaryExpression)node;
                    return new LogicalBinaryExpression(CloneExpression(binary.Left), binary.OperatorKind, binary.Result, CloneExpression(binary.Right));
                case LogicalExpressionKind.Conversion:
                    var conversion = (LogicalConversionExpression)node;
                    return new LogicalConversionExpression(CloneExpression(conversion.Expression), conversion.Type, conversion.Conversion);
                case LogicalExpressionKind.IsNull:
                    var isNull = (LogicalIsNullExpression)node;
                    return new LogicalIsNullExpression(CloneExpression(isNull.Expression));
                case LogicalExpressionKind.Case:
                    var caseExpression = (LogicalCaseExpression)node;
                    var labels = caseExpression.CaseLabels
                                               .Select(l => new LogicalCaseLabel(CloneExpression(l.Condition), CloneExpression(l.ThenExpression)))
                                               .ToImmutableArray();
                    var elseExpression = caseExpression.ElseExpression is null ? null : CloneExpression(caseExpression.ElseExpression);
                    return new LogicalCaseExpression(labels, elseExpression);
                case LogicalExpressionKind.FunctionInvocation:
                    var function = (LogicalFunctionInvocationExpression)node;
                    return new LogicalFunctionInvocationExpression(function.Arguments.Select(CloneExpression).ToImmutableArray(), function.Result);
                case LogicalExpressionKind.PropertyAccess:
                    var property = (LogicalPropertyAccessExpression)node;
                    return new LogicalPropertyAccessExpression(CloneExpression(property.Target), property.Symbol);
                case LogicalExpressionKind.MethodInvocation:
                    var method = (LogicalMethodInvocationExpression)node;
                    return new LogicalMethodInvocationExpression(CloneExpression(method.Target), method.Arguments.Select(CloneExpression).ToImmutableArray(), method.Result);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }
    }
}
