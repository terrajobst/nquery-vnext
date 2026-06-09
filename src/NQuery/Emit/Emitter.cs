#nullable enable

using System.Collections.Immutable;
using System.Linq.Expressions;

using NQuery.Planning;
using NQuery.Symbols;

namespace NQuery.Emit
{
    // The Emit phase: lowers the physical operator tree into an ExecutablePlan -- a
    // tree of ExecutableOperators ready to produce runtime iterators. Table-scan
    // column accessors are compiled here (they hold no row buffer, so they are
    // reusable); the remaining expressions are compiled per CreateIterator for now.
    internal static class Emitter
    {
        public static ExecutablePlan Emit(PhysicalQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            return new ExecutablePlan(EmitOperator(query.Root), query.OutputColumns);
        }

        public static ExecutablePlan Emit(PhysicalOperator root, ImmutableArray<QueryColumnInstanceSymbol> outputColumns)
        {
            ArgumentNullException.ThrowIfNull(root);

            return new ExecutablePlan(EmitOperator(root), outputColumns);
        }

        private static ExecutableOperator EmitOperator(PhysicalOperator node)
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
                    return EmitFilter((PhysicalFilter)node);
                case PhysicalOperatorKind.ComputeScalar:
                    return EmitComputeScalar((PhysicalComputeScalar)node);
                case PhysicalOperatorKind.Project:
                    return EmitProject((PhysicalProject)node);
                case PhysicalOperatorKind.Sort:
                    return EmitSort((PhysicalSort)node);
                case PhysicalOperatorKind.Top:
                    return EmitTop((PhysicalTop)node);
                case PhysicalOperatorKind.Join:
                    return EmitJoin((PhysicalJoin)node);
                case PhysicalOperatorKind.Apply:
                    return EmitApply((PhysicalApply)node);
                case PhysicalOperatorKind.Aggregate:
                    return EmitAggregate((PhysicalAggregate)node);
                case PhysicalOperatorKind.Concatenation:
                    return EmitConcatenation((PhysicalConcatenation)node);
                case PhysicalOperatorKind.IntersectOrExcept:
                    return EmitIntersectOrExcept((PhysicalIntersectOrExcept)node);
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

        private static ExecutableOperator EmitFilter(PhysicalFilter node)
        {
            return new ExecutableFilter(node.OutputValueSlots, EmitOperator(node.Input), node.Conditions);
        }

        private static ExecutableOperator EmitComputeScalar(PhysicalComputeScalar node)
        {
            return new ExecutableComputeScalar(node.OutputValueSlots, EmitOperator(node.Input), node.DefinedValues);
        }

        private static ExecutableOperator EmitProject(PhysicalProject node)
        {
            return new ExecutableProject(node.OutputValueSlots, EmitOperator(node.Input), node.Outputs);
        }

        private static ExecutableOperator EmitSort(PhysicalSort node)
        {
            return new ExecutableSort(node.OutputValueSlots, EmitOperator(node.Input), node.IsDistinct, node.SortedValues);
        }

        private static ExecutableOperator EmitTop(PhysicalTop node)
        {
            return new ExecutableTop(node.OutputValueSlots, EmitOperator(node.Input), node.Limit, node.TieEntries);
        }

        private static ExecutableOperator EmitJoin(PhysicalJoin node)
        {
            return new ExecutableJoin(node.OutputValueSlots, EmitOperator(node.Left), EmitOperator(node.Right));
        }

        private static ExecutableOperator EmitApply(PhysicalApply node)
        {
            return new ExecutableApply(node.OutputValueSlots, EmitOperator(node.Left), EmitOperator(node.Right));
        }

        private static ExecutableOperator EmitAggregate(PhysicalAggregate node)
        {
            return new ExecutableAggregate(node.OutputValueSlots, EmitOperator(node.Input));
        }

        private static ExecutableOperator EmitConcatenation(PhysicalConcatenation node)
        {
            var inputs = node.Inputs.Select(EmitOperator).ToImmutableArray();
            return new ExecutableConcatenation(node.OutputValueSlots, inputs);
        }

        private static ExecutableOperator EmitIntersectOrExcept(PhysicalIntersectOrExcept node)
        {
            return new ExecutableIntersectOrExcept(node.OutputValueSlots, EmitOperator(node.Left), EmitOperator(node.Right));
        }
    }
}
