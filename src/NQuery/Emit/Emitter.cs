#nullable enable

using System.Collections.Immutable;
using System.Linq.Expressions;

using NQuery.Binding;
using NQuery.Planning;
using NQuery.Symbols;

namespace NQuery.Emit
{
    // The Emit phase: lowers the physical operator tree into an ExecutablePlan -- a
    // tree of ExecutableOperators ready to produce runtime iterators. Table-scan
    // column accessors and the filter/compute/join predicates are compiled here once,
    // since they take the row buffer as a parameter and are reusable across runs.
    //
    // outerSlots carries the slots an enclosing Apply makes available to its right
    // subtree (its correlated outer references). It accumulates through nested applies
    // and lets correlated filters/computes compile against the (outer ++ input) layout.
    internal static class Emitter
    {
        public static ExecutablePlan Emit(PhysicalQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            return new ExecutablePlan(EmitOperator(query.Root, ImmutableArray<ValueSlot>.Empty), query.OutputColumns);
        }

        public static ExecutablePlan Emit(PhysicalOperator root, ImmutableArray<QueryColumnInstanceSymbol> outputColumns)
        {
            ArgumentNullException.ThrowIfNull(root);

            return new ExecutablePlan(EmitOperator(root, ImmutableArray<ValueSlot>.Empty), outputColumns);
        }

        private static ExecutableOperator EmitOperator(PhysicalOperator node, ImmutableArray<ValueSlot> outerSlots)
        {
            switch (node.Kind)
            {
                case PhysicalOperatorKind.Empty:
                    return new ExecutableEmpty(node.OutputValueSlots);
                case PhysicalOperatorKind.Constant:
                    return new ExecutableConstant(node.OutputValueSlots);
                case PhysicalOperatorKind.TableScan:
                    return EmitTableScan((PhysicalTableScan)node);
                case PhysicalOperatorKind.Filter:
                    return EmitFilter((PhysicalFilter)node, outerSlots);
                case PhysicalOperatorKind.ComputeScalar:
                    return EmitComputeScalar((PhysicalComputeScalar)node, outerSlots);
                case PhysicalOperatorKind.Project:
                    return EmitProject((PhysicalProject)node, outerSlots);
                case PhysicalOperatorKind.Sort:
                    return EmitSort((PhysicalSort)node, outerSlots);
                case PhysicalOperatorKind.Top:
                    return EmitTop((PhysicalTop)node, outerSlots);
                case PhysicalOperatorKind.NestedLoops:
                    return EmitNestedLoops((PhysicalNestedLoops)node, outerSlots);
                case PhysicalOperatorKind.StreamAggregates:
                    return EmitStreamAggregates((PhysicalStreamAggregates)node, outerSlots);
                case PhysicalOperatorKind.Concatenation:
                    return EmitConcatenation((PhysicalConcatenation)node, outerSlots);
                case PhysicalOperatorKind.Assert:
                    return EmitAssert((PhysicalAssert)node, outerSlots);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private static ExecutableOperator EmitTableScan(PhysicalTableScan node)
        {
            var schemaTable = (SchemaTableSymbol)node.TableInstance.Table;
            var accessors = node.DefinedValues
                                .Select(ci => ci.Column)
                                .Cast<SchemaColumnSymbol>()
                                .Select(c => BuildColumnAccess(c.Definition))
                                .ToImmutableArray();
            return new ExecutableTableScan(node.OutputValueSlots, schemaTable.Definition, accessors);
        }

        private static Func<object, object> BuildColumnAccess(ColumnDefinition definition)
        {
            var instance = Expression.Parameter(typeof(object));
            var body = definition.CreateInvocation(instance);
            return Expression.Lambda<Func<object, object>>(body, instance).Compile();
        }

        private static ExecutableOperator EmitFilter(PhysicalFilter node, ImmutableArray<ValueSlot> outerSlots)
        {
            return new ExecutableFilter(node.OutputValueSlots, EmitOperator(node.Input, outerSlots), node.Conditions, outerSlots);
        }

        private static ExecutableOperator EmitComputeScalar(PhysicalComputeScalar node, ImmutableArray<ValueSlot> outerSlots)
        {
            return new ExecutableComputeScalar(node.OutputValueSlots, EmitOperator(node.Input, outerSlots), node.DefinedValues, outerSlots);
        }

        private static ExecutableOperator EmitProject(PhysicalProject node, ImmutableArray<ValueSlot> outerSlots)
        {
            return new ExecutableProject(node.OutputValueSlots, EmitOperator(node.Input, outerSlots), node.Outputs);
        }

        private static ExecutableOperator EmitSort(PhysicalSort node, ImmutableArray<ValueSlot> outerSlots)
        {
            return new ExecutableSort(node.OutputValueSlots, EmitOperator(node.Input, outerSlots), node.IsDistinct, node.SortedValues);
        }

        private static ExecutableOperator EmitTop(PhysicalTop node, ImmutableArray<ValueSlot> outerSlots)
        {
            return new ExecutableTop(node.OutputValueSlots, EmitOperator(node.Input, outerSlots), node.Limit, node.TieEntries);
        }

        private static ExecutableOperator EmitNestedLoops(PhysicalNestedLoops node, ImmutableArray<ValueSlot> outerSlots)
        {
            var left = EmitOperator(node.Left, outerSlots);

            // A dependent join (apply) adds its outer references to the right subtree's
            // outer scope, after any outer this node itself sits under. A plain join's
            // right is independent, so it just sees the inherited outer scope.
            var rightOuterSlots = node.OuterReferences.IsEmpty
                ? outerSlots
                : outerSlots.AddRange(node.OuterReferences);
            var right = EmitOperator(node.Right, rightOuterSlots);
            return new ExecutableNestedLoops(node.OutputValueSlots, left, right, node.JoinKind, node.Conditions, node.Probe, node.PassthruPredicate, node.OuterReferences);
        }

        private static ExecutableOperator EmitStreamAggregates(PhysicalStreamAggregates node, ImmutableArray<ValueSlot> outerSlots)
        {
            return new ExecutableStreamAggregates(node.OutputValueSlots, EmitOperator(node.Input, outerSlots), node.Groups, node.Aggregates, outerSlots);
        }

        private static ExecutableOperator EmitConcatenation(PhysicalConcatenation node, ImmutableArray<ValueSlot> outerSlots)
        {
            var inputs = node.Inputs.Select(i => EmitOperator(i, outerSlots)).ToImmutableArray();
            return new ExecutableConcatenation(node.OutputValueSlots, inputs, node.DefinedValues);
        }

        private static ExecutableOperator EmitAssert(PhysicalAssert node, ImmutableArray<ValueSlot> outerSlots)
        {
            return new ExecutableAssert(node.OutputValueSlots, EmitOperator(node.Input, outerSlots), node.Condition, node.Message, outerSlots);
        }
    }
}
