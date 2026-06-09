#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;

namespace NQuery.Planning
{
    // Translates the optimized logical algebra into a physical operator tree
    // (Logical -> Physical). Mostly a one-to-one lowering; the real planning
    // decision so far is choosing a join algorithm (hash match vs nested loops).
    // The stream-vs-hash aggregate choice and cost-based decisions are future work.
    internal static class Planner
    {
        public static PhysicalQuery Plan(LogicalQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var root = Plan(query.Root);
            return new PhysicalQuery(root, query.OutputColumns);
        }

        public static PhysicalOperator Plan(LogicalOperator root)
        {
            ArgumentNullException.ThrowIfNull(root);

            return PlanOperator(root);
        }

        private static PhysicalOperator PlanOperator(LogicalOperator node)
        {
            switch (node.Kind)
            {
                case LogicalOperatorKind.Empty:
                    return PlanEmpty((LogicalEmpty)node);
                case LogicalOperatorKind.Constant:
                    return PlanConstant((LogicalConstant)node);
                case LogicalOperatorKind.TableScan:
                    return PlanTableScan((LogicalTableScan)node);
                case LogicalOperatorKind.Filter:
                    return PlanFilter((LogicalFilter)node);
                case LogicalOperatorKind.Compute:
                    return PlanCompute((LogicalCompute)node);
                case LogicalOperatorKind.Project:
                    return PlanProject((LogicalProject)node);
                case LogicalOperatorKind.Join:
                    return PlanJoin((LogicalJoin)node);
                case LogicalOperatorKind.Apply:
                    return PlanApply((LogicalApply)node);
                case LogicalOperatorKind.Aggregate:
                    return PlanAggregate((LogicalAggregate)node);
                case LogicalOperatorKind.Union:
                    return PlanUnion((LogicalUnion)node);
                case LogicalOperatorKind.IntersectOrExcept:
                    return PlanIntersectOrExcept((LogicalIntersectOrExcept)node);
                case LogicalOperatorKind.Sort:
                    return PlanSort((LogicalSort)node);
                case LogicalOperatorKind.Top:
                    return PlanTop((LogicalTop)node);
                default:
                    throw ExceptionBuilder.UnexpectedValue(node.Kind);
            }
        }

        private static PhysicalOperator PlanEmpty(LogicalEmpty node) => new PhysicalEmpty();

        private static PhysicalOperator PlanConstant(LogicalConstant node) => new PhysicalConstant();

        private static PhysicalOperator PlanTableScan(LogicalTableScan node) => new PhysicalTableScan(node.TableInstance, node.DefinedValues);

        private static PhysicalOperator PlanFilter(LogicalFilter node)
        {
            var input = PlanOperator(node.Input);
            return new PhysicalFilter(input, node.Conditions);
        }

        private static PhysicalOperator PlanCompute(LogicalCompute node)
        {
            var input = PlanOperator(node.Input);
            return new PhysicalComputeScalar(input, node.DefinedValues);
        }

        private static PhysicalOperator PlanProject(LogicalProject node)
        {
            var input = PlanOperator(node.Input);
            return new PhysicalProject(input, node.Outputs);
        }

        private static PhysicalOperator PlanJoin(LogicalJoin node)
        {
            var left = PlanOperator(node.Left);
            var right = PlanOperator(node.Right);
            var algorithm = ChooseJoinAlgorithm(node, left, right);
            return new PhysicalJoin(algorithm, node.JoinKind, left, right, node.Conditions, node.Probe, node.PassthruPredicate);
        }

        private static PhysicalOperator PlanApply(LogicalApply node)
        {
            var left = PlanOperator(node.Left);
            var right = PlanOperator(node.Right);
            return new PhysicalApply(node.ApplyKind, left, right, node.Probe);
        }

        private static PhysicalOperator PlanAggregate(LogicalAggregate node)
        {
            var input = PlanOperator(node.Input);
            return new PhysicalAggregate(input, node.Groups, node.Aggregates);
        }

        private static PhysicalOperator PlanUnion(LogicalUnion node)
        {
            var inputs = node.Inputs.Select(PlanOperator).ToImmutableArray();
            return new PhysicalConcatenation(node.IsUnionAll, inputs, node.DefinedValues, node.Comparers);
        }

        private static PhysicalOperator PlanIntersectOrExcept(LogicalIntersectOrExcept node)
        {
            var left = PlanOperator(node.Left);
            var right = PlanOperator(node.Right);
            return new PhysicalIntersectOrExcept(node.IsIntersect, left, right, node.Comparers);
        }

        private static PhysicalOperator PlanSort(LogicalSort node)
        {
            var input = PlanOperator(node.Input);
            return new PhysicalSort(node.IsDistinct, input, node.SortedValues);
        }

        private static PhysicalOperator PlanTop(LogicalTop node)
        {
            var input = PlanOperator(node.Input);
            return new PhysicalTop(input, node.Limit, node.TieEntries);
        }

        // A join with a conjunct that equates a left-only expression with a
        // right-only expression can use a hash match; otherwise nested loops.
        private static PhysicalJoinAlgorithm ChooseJoinAlgorithm(LogicalJoin node, PhysicalOperator left, PhysicalOperator right)
        {
            foreach (var condition in node.Conditions)
            {
                if (IsEquiJoinCondition(condition, left.DefinedValueSlots, right.DefinedValueSlots))
                    return PhysicalJoinAlgorithm.HashMatch;
            }

            return PhysicalJoinAlgorithm.NestedLoops;
        }

        private static bool IsEquiJoinCondition(LogicalExpression condition, FrozenSet<ValueSlot> left, FrozenSet<ValueSlot> right)
        {
            if (condition is not LogicalBinaryExpression { OperatorKind: BinaryOperatorKind.Equal } binary)
                return false;

            var operands = LogicalSlotReferenceFinder.FindReferencedSlots(binary.Left);
            var others = LogicalSlotReferenceFinder.FindReferencedSlots(binary.Right);
            if (operands.Count == 0 || others.Count == 0)
                return false;

            return (operands.All(left.Contains) && others.All(right.Contains))
                || (operands.All(right.Contains) && others.All(left.Contains));
        }
    }
}
