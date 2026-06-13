#nullable enable

using System.Collections.Immutable;

using NQuery.Refactor.Binding;
using NQuery.Symbols;
using NQuery.Symbols.Aggregation;

using BinaryOperator = NQuery.Binding.BinaryOperator;
using BinaryOperatorKind = NQuery.Binding.BinaryOperatorKind;
using UnaryOperator = NQuery.Binding.UnaryOperator;
using UnaryOperatorKind = NQuery.Binding.UnaryOperatorKind;

namespace NQuery.Refactor.Algebra
{
    // Translates the binder's syntax-shaped query tree (BoundQuery) into the logical algebra
    // (LogicalOperator). This is the Bound -> Logical lowering.
    //
    // It reuses the binder's value slots and resolved operator/symbol metadata. Two things
    // happen here that the binder deliberately left undone:
    //
    //   * a SELECT query, kept per-clause by the binder, is lowered into the relational
    //     pipeline (Filter/Compute/Aggregate/Sort/Top/Project); and
    //   * correlation is made explicit -- a scalar subquery becomes a LeftOuter Apply whose
    //     value slot the expression then references, and an EXISTS subquery becomes a Semi Apply
    //     producing a boolean probe slot the expression references. The logical expression
    //     language therefore cannot contain a query: subqueries are never expression nodes.
    //
    // Apply introduction happens here because it *is* part of forming the algebra. Decorrelation
    // -- pushing those applies down until the right no longer depends on the left, turning them
    // into ordinary joins -- is a separate optimizer pass.
    //
    // Out of scope: subqueries inside a non-inner join's ON condition -- these throw.
    internal sealed class Algebrizer
    {
        private readonly ValueSlotFactory _valueSlotFactory = new();

        // The CASE-branch passthru guards in scope while algebrizing an expression. A
        // subquery found here is skipped (passed through its Apply) for rows the guard
        // matches. A pushed null means "no guard"; CurrentPassthru reads the top.
        private readonly Stack<LogicalExpression?> _passthruStack = new();

        // The single slot-assignment authority: every IBoundValue the binder produced maps to
        // exactly one algebra ValueSlot, minted on first use. This is the Bound -> Logical
        // value-identity translation; nothing downstream of the algebrizer sees an IBoundValue.
        private readonly Dictionary<IBoundValue, ValueSlot> _slots = new();

        public static LogicalQuery Algebrize(BoundQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var algebrizer = new Algebrizer();
            var root = algebrizer.AlgebrizeQuery(query);
            return new LogicalQuery(root, query.OutputColumns);
        }

        // A bare expression (Expression<T>) has no query around it. Wrap it the way the legacy
        // pipeline did (Compilation.CreateBoundQuery): evaluate it as the single computed column
        // of a one-row (constant) relation, then project that column. The resulting LogicalQuery
        // flows through Optimize/Plan/Emit exactly like a FROM-less SELECT. A subquery in the
        // expression is fine -- AlgebrizeCompute hoists its Apply above the constant.
        public static LogicalQuery Algebrize(BoundExpression expression)
        {
            ArgumentNullException.ThrowIfNull(expression);

            var algebrizer = new Algebrizer();
            return algebrizer.AlgebrizeBareExpression(expression);
        }

        private LogicalQuery AlgebrizeBareExpression(BoundExpression expression)
        {
            var result = new BoundValue(@"result", expression.Type);
            var computedValue = new BoundComputedValue(expression, result);

            LogicalOperator input = new LogicalConstant();
            input = AlgebrizeCompute(input, ImmutableArray.Create(computedValue));
            input = new LogicalProject(input, ImmutableArray.Create(GetSlot(result)));

            var outputColumn = new QueryColumnInstanceSymbol(result.Name, result);
            return new LogicalQuery(input, ImmutableArray.Create(outputColumn));
        }

        private LogicalOperator AlgebrizeQuery(BoundQuery node)
        {
            return node switch
            {
                BoundSelectQuery select => AlgebrizeSelectQuery(select),
                BoundUnionQuery union => AlgebrizeUnionQuery(union),
                BoundIntersectOrExceptQuery intersectOrExcept => AlgebrizeIntersectOrExceptQuery(intersectOrExcept),
                BoundOrderedQuery ordered => AlgebrizeOrderedQuery(ordered),
                BoundEmptyQuery => new LogicalEmpty(),
                _ => throw ExceptionBuilder.UnexpectedValue(node.Kind)
            };
        }

        // Lowers a per-clause SELECT into the relational pipeline. The order mirrors the legacy
        // binder's "putting it together" assembly: FROM -> WHERE -> compute group expressions ->
        // GROUP BY/aggregate -> HAVING -> compute SELECT expressions -> sort -> top -> project ->
        // distinct (when there is no ORDER BY).
        private LogicalOperator AlgebrizeSelectQuery(BoundSelectQuery node)
        {
            var input = node.From is null
                ? new LogicalConstant()
                : AlgebrizeTableReference(node.From.Root);

            if (node.Where is not null)
                input = AlgebrizeFilter(input, node.Where.Condition);

            if (node.GroupBy is not null && !node.GroupBy.ComputedGroups.IsEmpty)
                input = AlgebrizeCompute(input, node.GroupBy.ComputedGroups);

            var groups = node.GroupBy is null
                ? ImmutableArray<LogicalComparedValue>.Empty
                : node.GroupBy.Groups.Select(TranslateCompared).ToImmutableArray();
            var aggregates = node.Select.Aggregates;
            if (!groups.IsEmpty || !aggregates.IsEmpty)
            {
                var applies = new List<PendingApply>();
                var aggs = aggregates.Select(a => AlgebrizeAggregatedValue(a, applies)).ToImmutableArray();
                input = WrapInApplies(input, applies);
                input = new LogicalAggregate(input, groups, aggs);
            }

            if (node.Having is not null)
                input = AlgebrizeFilter(input, node.Having.Condition);

            if (!node.Select.Projections.IsEmpty)
                input = AlgebrizeCompute(input, node.Select.Projections);

            if (node.OrderBy is not null)
                input = new LogicalSort(node.Select.IsDistinct, input, node.OrderBy.SortedValues.Select(TranslateCompared).ToImmutableArray());

            if (node.Top is not null)
            {
                // TOP WITH TIES breaks ties using the sort keys (the binder requires an ORDER BY
                // for WITH TIES, so OrderBy is non-null here).
                var tieEntries = node.Top.WithTies && node.OrderBy is not null
                    ? node.OrderBy.SortedValues.Select(TranslateCompared).ToImmutableArray()
                    : ImmutableArray<LogicalComparedValue>.Empty;
                input = new LogicalTop(input, node.Top.Limit, tieEntries);
            }

            input = new LogicalProject(input, node.OutputColumns.Select(c => GetSlot(c.BoundValue)).ToImmutableArray());

            // SELECT DISTINCT with no ORDER BY: a group-by over the output columns.
            if (!node.Select.DistinctValues.IsEmpty)
                input = new LogicalAggregate(input, node.Select.DistinctValues.Select(TranslateCompared).ToImmutableArray(), ImmutableArray<LogicalAggregatedValue>.Empty);

            return input;
        }

        private LogicalOperator AlgebrizeFilter(LogicalOperator input, BoundExpression condition)
        {
            var applies = new List<PendingApply>();
            var logicalCondition = AlgebrizeExpression(condition, applies);
            input = WrapInApplies(input, applies);
            return new LogicalFilter(input, FlattenConjuncts(logicalCondition).ToImmutableArray());
        }

        private LogicalOperator AlgebrizeCompute(LogicalOperator input, ImmutableArray<BoundComputedValue> definedValues)
        {
            var applies = new List<PendingApply>();
            var defined = definedValues.Select(v => AlgebrizeComputedValue(v, applies)).ToImmutableArray();
            input = WrapInApplies(input, applies);
            return new LogicalCompute(input, defined);
        }

        private LogicalOperator AlgebrizeTableReference(BoundTableReference node)
        {
            return node switch
            {
                BoundNamedTableReference named => AlgebrizeNamedTableReference(named),
                // A derived table is a pure scoping wrapper: its column slots are the inner query's
                // output slots, so it carries no algebraic meaning -- inline the inner query.
                BoundDerivedTableReference derived => AlgebrizeQuery(derived.Query),
                BoundJoinTableReference join => AlgebrizeJoinTableReference(join),
                _ => throw ExceptionBuilder.UnexpectedValue(node.Kind)
            };
        }

        private LogicalOperator AlgebrizeJoinTableReference(BoundJoinTableReference node)
        {
            var left = AlgebrizeTableReference(node.Left);
            var right = AlgebrizeTableReference(node.Right);

            // Split the ON condition into conjuncts. A subquery-free conjunct stays as the join's
            // own condition (so an equi-join can still become a hash match). A conjunct that
            // introduces a subquery turns into an Apply, which can't live inside the join
            // expression -- so it is hoisted into a filter above the join.
            var joinConditions = new List<LogicalExpression>();
            var filterConditions = new List<LogicalExpression>();
            var applies = new List<PendingApply>();

            foreach (var conjunct in SplitConjuncts(node.Condition))
            {
                var conjunctApplies = new List<PendingApply>();
                var algebrized = AlgebrizeExpression(conjunct, conjunctApplies);
                if (conjunctApplies.Count == 0)
                {
                    joinConditions.Add(algebrized);
                }
                else
                {
                    applies.AddRange(conjunctApplies);
                    filterConditions.Add(algebrized);
                }
            }

            var conditions = joinConditions.ToImmutableArray();

            // Normalize RIGHT OUTER to LEFT OUTER by swapping inputs, so the logical layer never
            // sees a right-outer join. The conditions reference slots by identity, so swapping the
            // operands preserves meaning.
            var join = node.JoinType == BoundJoinType.RightOuter
                ? new LogicalJoin(LogicalJoinKind.LeftOuter, right, left, conditions, probe: null, passthruPredicate: null)
                : new LogicalJoin(MapJoinKind(node.JoinType), left, right, conditions, probe: null, passthruPredicate: null);

            if (applies.Count == 0)
                return join;

            // Hoisting a conjunct out of the ON clause into a filter above the join only preserves
            // semantics for an inner join, where ON is a plain filter.
            if (node.JoinType != BoundJoinType.Inner)
                throw new NotSupportedException("Subqueries inside a non-inner join's ON condition are not yet supported.");

            var withApplies = WrapInApplies(join, applies);
            return new LogicalFilter(withApplies, filterConditions.ToImmutableArray());
        }

        private LogicalOperator AlgebrizeUnionQuery(BoundUnionQuery node)
        {
            var inputs = node.Inputs.Select(AlgebrizeQueryInput).ToImmutableArray();
            var unifiedValues = node.UnifiedValues.Select(TranslateUnified).ToImmutableArray();
            return new LogicalUnion(node.IsUnionAll, inputs, unifiedValues, node.Comparers);
        }

        private LogicalOperator AlgebrizeIntersectOrExceptQuery(BoundIntersectOrExceptQuery node)
        {
            var left = AlgebrizeQueryInput(node.Left);
            var right = AlgebrizeQueryInput(node.Right);
            return new LogicalIntersectOrExcept(node.IsIntersect, left, right, node.Comparers);
        }

        private LogicalOperator AlgebrizeOrderedQuery(BoundOrderedQuery node)
        {
            var input = AlgebrizeQuery(node.Query);
            return new LogicalSort(false, input, node.SortedValues.Select(TranslateCompared).ToImmutableArray());
        }

        // A set-operation input: the inner query, plus the type-coercion compute/project the
        // binder deferred (empty when no coercion was needed).
        private LogicalOperator AlgebrizeQueryInput(BoundQueryInput input)
        {
            var relation = AlgebrizeQuery(input.Query);
            if (input.ComputedValues.IsEmpty)
                return relation;

            relation = AlgebrizeCompute(relation, input.ComputedValues);
            return new LogicalProject(relation, input.OutputValues.Select(GetSlot).ToImmutableArray());
        }

        private static LogicalJoinKind MapJoinKind(BoundJoinType joinType)
        {
            return joinType switch
            {
                BoundJoinType.Inner => LogicalJoinKind.Inner,
                BoundJoinType.LeftOuter => LogicalJoinKind.LeftOuter,
                BoundJoinType.FullOuter => LogicalJoinKind.FullOuter,
                BoundJoinType.LeftSemi => LogicalJoinKind.LeftSemi,
                BoundJoinType.LeftAntiSemi => LogicalJoinKind.LeftAntiSemi,
                _ => throw ExceptionBuilder.UnexpectedValue(joinType)
            };
        }

        // Splits a bound predicate into its top-level conjuncts (an AND chain). Used to keep
        // subquery-free join conditions on the join while pulling subquery-bearing ones above it.
        private static IEnumerable<BoundExpression> SplitConjuncts(BoundExpression? expression)
        {
            if (expression is null)
                yield break;

            if (expression is BoundBinaryExpression { OperatorKind: BinaryOperatorKind.LogicalAnd } binary)
            {
                foreach (var conjunct in SplitConjuncts(binary.Left))
                    yield return conjunct;
                foreach (var conjunct in SplitConjuncts(binary.Right))
                    yield return conjunct;
            }
            else
            {
                yield return expression;
            }
        }

        // Splits a logical predicate into its top-level conjuncts so that Filter/Join can store
        // them as a list (an implicit AND), which is what selection pushdown moves individually.
        private static IEnumerable<LogicalExpression> FlattenConjuncts(LogicalExpression? expression)
        {
            if (expression is null)
                yield break;

            if (expression is LogicalBinaryExpression { OperatorKind: BinaryOperatorKind.LogicalAnd } binary)
            {
                foreach (var conjunct in FlattenConjuncts(binary.Left))
                    yield return conjunct;
                foreach (var conjunct in FlattenConjuncts(binary.Right))
                    yield return conjunct;
            }
            else
            {
                yield return expression;
            }
        }

        private LogicalComputedValue AlgebrizeComputedValue(BoundComputedValue value, List<PendingApply> applies)
        {
            var expression = AlgebrizeExpression(value.Expression, applies);
            return new LogicalComputedValue(expression, GetSlot(value.Value));
        }

        private LogicalAggregatedValue AlgebrizeAggregatedValue(BoundAggregatedValue value, List<PendingApply> applies)
        {
            var argument = AlgebrizeExpression(value.Argument, applies);
            return new LogicalAggregatedValue(GetSlot(value.Output), value.Aggregate, value.Aggregatable, argument);
        }

        // Each operator collects the applies produced while translating its own expressions, then
        // wraps its input in them so the subquery slots are in scope for the expressions that
        // reference them.
        private static LogicalOperator WrapInApplies(LogicalOperator input, List<PendingApply> applies)
        {
            foreach (var apply in applies)
                input = new LogicalApply(apply.Kind, input, apply.Relation, apply.Probe, apply.Passthru);
            return input;
        }

        private LogicalExpression? AlgebrizeExpressionOrNull(BoundExpression? node, List<PendingApply> applies)
        {
            return node is null ? null : AlgebrizeExpression(node, applies);
        }

        private LogicalExpression AlgebrizeExpression(BoundExpression node, List<PendingApply> applies)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    return AlgebrizeLiteralExpression((BoundLiteralExpression)node, applies);
                case BoundNodeKind.ValueExpression:
                    return AlgebrizeValueExpression((BoundValueExpression)node, applies);
                case BoundNodeKind.ColumnExpression:
                    return AlgebrizeColumnExpression((BoundColumnExpression)node, applies);
                case BoundNodeKind.VariableExpression:
                    return AlgebrizeVariableExpression((BoundVariableExpression)node, applies);
                case BoundNodeKind.UnaryExpression:
                    return AlgebrizeUnaryExpression((BoundUnaryExpression)node, applies);
                case BoundNodeKind.BinaryExpression:
                    return AlgebrizeBinaryExpression((BoundBinaryExpression)node, applies);
                case BoundNodeKind.ConversionExpression:
                    return AlgebrizeConversionExpression((BoundConversionExpression)node, applies);
                case BoundNodeKind.IsNullExpression:
                    return AlgebrizeIsNullExpression((BoundIsNullExpression)node, applies);
                case BoundNodeKind.CaseExpression:
                    return AlgebrizeCaseExpression((BoundCaseExpression)node, applies);
                case BoundNodeKind.FunctionInvocationExpression:
                    return AlgebrizeFunctionInvocationExpression((BoundFunctionInvocationExpression)node, applies);
                case BoundNodeKind.PropertyAccessExpression:
                    return AlgebrizePropertyAccessExpression((BoundPropertyAccessExpression)node, applies);
                case BoundNodeKind.MethodInvocationExpression:
                    return AlgebrizeMethodInvocationExpression((BoundMethodInvocationExpression)node, applies);
                case BoundNodeKind.SingleRowSubselect:
                    return AlgebrizeSingleRowSubselect((BoundSingleRowSubselect)node, applies);
                case BoundNodeKind.ExistsSubselect:
                    return AlgebrizeExistsSubselect((BoundExistsSubselect)node, applies);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private LogicalExpression AlgebrizeLiteralExpression(BoundLiteralExpression node, List<PendingApply> applies)
        {
            return new LogicalLiteralExpression(node.Value);
        }

        private LogicalExpression AlgebrizeValueExpression(BoundValueExpression node, List<PendingApply> applies)
        {
            return new LogicalValueSlotExpression(GetSlot(node.Value));
        }

        // A column reference resolves to a reference to the slot its symbol owns: this is the
        // "symbol-refs become slot-refs" step of algebrization.
        private LogicalExpression AlgebrizeColumnExpression(BoundColumnExpression node, List<PendingApply> applies)
        {
            return new LogicalValueSlotExpression(GetSlot(node.Symbol.BoundValue));
        }

        private LogicalExpression AlgebrizeVariableExpression(BoundVariableExpression node, List<PendingApply> applies)
        {
            return new LogicalVariableExpression(node.Symbol);
        }

        private LogicalExpression AlgebrizeUnaryExpression(BoundUnaryExpression node, List<PendingApply> applies)
        {
            var expression = AlgebrizeExpression(node.Expression, applies);
            return new LogicalUnaryExpression(node.OperatorKind, node.Result, expression);
        }

        private LogicalExpression AlgebrizeBinaryExpression(BoundBinaryExpression node, List<PendingApply> applies)
        {
            var left = AlgebrizeExpression(node.Left, applies);
            var right = AlgebrizeExpression(node.Right, applies);
            return new LogicalBinaryExpression(left, node.OperatorKind, node.Result, right);
        }

        private LogicalExpression AlgebrizeConversionExpression(BoundConversionExpression node, List<PendingApply> applies)
        {
            var expression = AlgebrizeExpression(node.Expression, applies);
            return new LogicalConversionExpression(expression, node.Type, node.Conversion);
        }

        private LogicalExpression AlgebrizeIsNullExpression(BoundIsNullExpression node, List<PendingApply> applies)
        {
            var expression = AlgebrizeExpression(node.Expression, applies);
            return new LogicalIsNullExpression(expression);
        }

        // CASE short-circuits: a subquery in a THEN/ELSE is evaluated only when its branch is
        // selected. We thread a "passthru" guard -- the condition under which a branch is *not*
        // reached -- through the branch, and AlgebrizeSingleRowSubselect / AlgebrizeExistsSubselect
        // stamp it onto any Apply they introduce, so the executor skips the subquery for those rows.
        private LogicalExpression AlgebrizeCaseExpression(BoundCaseExpression node, List<PendingApply> applies)
        {
            var whenPassthru = CurrentPassthru;
            var caseLabels = ImmutableArray.CreateBuilder<LogicalCaseLabel>(node.CaseLabels.Length);

            foreach (var label in node.CaseLabels)
            {
                _passthruStack.Push(whenPassthru);
                var when = AlgebrizeExpression(label.Condition, applies);
                _passthruStack.Pop();

                _passthruStack.Push(Or(whenPassthru, Not(when)));
                var then = AlgebrizeExpression(label.ThenExpression, applies);
                _passthruStack.Pop();

                caseLabels.Add(new LogicalCaseLabel(when, then));

                whenPassthru = Or(whenPassthru, when);
            }

            _passthruStack.Push(whenPassthru);
            var elseExpression = AlgebrizeExpressionOrNull(node.ElseExpression, applies);
            _passthruStack.Pop();

            return new LogicalCaseExpression(caseLabels.ToImmutable(), elseExpression);
        }

        private LogicalExpression? CurrentPassthru => _passthruStack.Count == 0 ? null : _passthruStack.Peek();

        private static LogicalExpression Not(LogicalExpression expression)
        {
            var result = UnaryOperator.Resolve(UnaryOperatorKind.LogicalNot, expression.Type);
            return new LogicalUnaryExpression(UnaryOperatorKind.LogicalNot, result, expression);
        }

        private static LogicalExpression? Or(LogicalExpression? left, LogicalExpression right)
        {
            return left is null ? right : Binary(left, BinaryOperatorKind.LogicalOr, right);
        }

        private LogicalExpression AlgebrizeFunctionInvocationExpression(BoundFunctionInvocationExpression node, List<PendingApply> applies)
        {
            var arguments = node.Arguments.Select(a => AlgebrizeExpression(a, applies)).ToImmutableArray();
            return new LogicalFunctionInvocationExpression(arguments, node.Result);
        }

        private LogicalExpression AlgebrizePropertyAccessExpression(BoundPropertyAccessExpression node, List<PendingApply> applies)
        {
            var target = AlgebrizeExpression(node.Target, applies);
            return new LogicalPropertyAccessExpression(target, node.Symbol);
        }

        private LogicalExpression AlgebrizeMethodInvocationExpression(BoundMethodInvocationExpression node, List<PendingApply> applies)
        {
            var target = AlgebrizeExpression(node.Target, applies);
            var arguments = node.Arguments.Select(a => AlgebrizeExpression(a, applies)).ToImmutableArray();
            return new LogicalMethodInvocationExpression(target, arguments, node.Result);
        }

        // A scalar subquery becomes a LeftOuter apply that exposes its value slot; the expression
        // is replaced by a reference to that slot.
        private LogicalExpression AlgebrizeSingleRowSubselect(BoundSingleRowSubselect node, List<PendingApply> applies)
        {
            var relation = AlgebrizeQuery(node.Query);
            var value = GetSlot(node.Value);

            // A scalar subquery must yield at most one row. When the query already guarantees that,
            // attach it directly. Otherwise guard it with a cardinality check.
            if (ReturnsAtMostOneRow(relation))
            {
                applies.Add(new PendingApply(LogicalApplyKind.LeftOuter, relation, probe: null, CurrentPassthru));
                return new LogicalValueSlotExpression(value);
            }

            var guarded = GuardSingleRow(relation, value, out var output);
            applies.Add(new PendingApply(LogicalApplyKind.LeftOuter, guarded, probe: null, CurrentPassthru));
            return new LogicalValueSlotExpression(output);
        }

        // ANY(value) collapses the (zero or one) surviving rows to the scalar result; COUNT detects
        // the error case. With no GROUP BY the aggregate always yields a single row, so the assert
        // sees one row carrying both the value and the count.
        private LogicalOperator GuardSingleRow(LogicalOperator relation, ValueSlot value, out ValueSlot output)
        {
            var anyOutput = _valueSlotFactory.CreateTemporary(value.Type);
            var countOutput = _valueSlotFactory.CreateTemporary(typeof(int));

            var any = new LogicalAggregatedValue(
                anyOutput,
                BuiltInAggregates.Any,
                BuiltInAggregates.Any.Definition.CreateAggregatable(value.Type),
                new LogicalValueSlotExpression(value));

            var count = new LogicalAggregatedValue(
                countOutput,
                BuiltInAggregates.Count,
                BuiltInAggregates.Count.Definition.CreateAggregatable(typeof(int)),
                new LogicalLiteralExpression(0));

            var aggregate = new LogicalAggregate(relation, ImmutableArray<LogicalComparedValue>.Empty, ImmutableArray.Create(any, count));

            var condition = Binary(new LogicalValueSlotExpression(countOutput), BinaryOperatorKind.LessOrEqual, new LogicalLiteralExpression(1));

            output = anyOutput;
            return new LogicalAssert(aggregate, condition, Resources.SubqueryReturnedMoreThanRow);
        }

        // Whether a relation provably returns at most one row, so the cardinality guard can be
        // skipped. Conservative: it peels cardinality-non-increasing unary operators down to a
        // capping node (a scalar aggregate, TOP 1, or a constant).
        private static bool ReturnsAtMostOneRow(LogicalOperator node)
        {
            switch (node.Kind)
            {
                case LogicalOperatorKind.Empty:
                case LogicalOperatorKind.Constant:
                    return true;
                case LogicalOperatorKind.Aggregate:
                    return ((LogicalAggregate)node).Groups.IsEmpty;
                case LogicalOperatorKind.Top:
                    var top = (LogicalTop)node;
                    return top.Limit <= 1 && top.TieEntries.IsEmpty || ReturnsAtMostOneRow(top.Input);
                case LogicalOperatorKind.Filter:
                    return ReturnsAtMostOneRow(((LogicalFilter)node).Input);
                case LogicalOperatorKind.Compute:
                    return ReturnsAtMostOneRow(((LogicalCompute)node).Input);
                case LogicalOperatorKind.Project:
                    return ReturnsAtMostOneRow(((LogicalProject)node).Input);
                case LogicalOperatorKind.Sort:
                    return ReturnsAtMostOneRow(((LogicalSort)node).Input);
                case LogicalOperatorKind.Assert:
                    return ReturnsAtMostOneRow(((LogicalAssert)node).Input);
                default:
                    return false;
            }
        }

        private static LogicalBinaryExpression Binary(LogicalExpression left, BinaryOperatorKind kind, LogicalExpression right)
        {
            var result = BinaryOperator.Resolve(kind, left.Type, right.Type);
            return new LogicalBinaryExpression(left, kind, result, right);
        }

        // EXISTS becomes a Semi apply that produces a boolean probe slot; the expression is
        // replaced by a reference to the probe (so NOT EXISTS is just NOT <probe>, and EXISTS can
        // appear anywhere in a predicate). ALL/ANY rewrites carry an extra compute/filter the
        // binder deferred; those are applied over the subquery before the semi apply.
        private LogicalExpression AlgebrizeExistsSubselect(BoundExistsSubselect node, List<PendingApply> applies)
        {
            var relation = AlgebrizeQuery(node.Query);

            if (!node.ComputedValues.IsEmpty)
                relation = AlgebrizeCompute(relation, node.ComputedValues);

            if (node.Filter is not null)
                relation = AlgebrizeFilter(relation, node.Filter);

            var probe = _valueSlotFactory.CreateTemporary(typeof(bool));
            applies.Add(new PendingApply(LogicalApplyKind.LeftSemi, relation, probe, CurrentPassthru));
            return new LogicalValueSlotExpression(probe);
        }

        // Maps an IBoundValue to its algebra slot, minting one the first time. Column references
        // resolve here too (the symbol's denoted value is the key), so the slot a table scan mints
        // for a column is the same one every reference to it sees.
        private ValueSlot GetSlot(IBoundValue value)
        {
            if (!_slots.TryGetValue(value, out var slot))
            {
                slot = MintSlot(value);
                _slots.Add(value, slot);
            }

            return slot;
        }

        private ValueSlot MintSlot(IBoundValue value)
        {
            return value is TableColumnInstanceSymbol column
                ? _valueSlotFactory.CreateNamed($"{column.TableInstance.Name}.{column.Name}", value.Type)
                : _valueSlotFactory.CreateTemporary(value.Type);
        }

        private LogicalComparedValue TranslateCompared(BoundComparedValue value)
        {
            return new LogicalComparedValue(GetSlot(value.Value), value.Comparer);
        }

        private LogicalUnifiedValue TranslateUnified(BoundUnifiedValue value)
        {
            return new LogicalUnifiedValue(GetSlot(value.Value), value.InputValues.Select(GetSlot));
        }

        private LogicalTableScan AlgebrizeNamedTableReference(BoundNamedTableReference node)
        {
            var slots = node.DefinedValues.Select(c => GetSlot(c.BoundValue)).ToImmutableArray();
            return new LogicalTableScan(node.TableInstance, node.DefinedValues, slots);
        }

        private readonly struct PendingApply
        {
            public PendingApply(LogicalApplyKind kind, LogicalOperator relation, ValueSlot? probe, LogicalExpression? passthru)
            {
                Kind = kind;
                Relation = relation;
                Probe = probe;
                Passthru = passthru;
            }

            public LogicalApplyKind Kind { get; }

            public LogicalOperator Relation { get; }

            public ValueSlot? Probe { get; }

            // The CASE-branch guard under which this subquery is skipped (null when the subquery
            // isn't inside a CASE branch).
            public LogicalExpression? Passthru { get; }
        }
    }
}
