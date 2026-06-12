#nullable enable

using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Binding;

namespace NQuery.Planning
{
    // Translates the optimized logical algebra into a physical operator tree
    // (Logical -> Physical). Currently a one-to-one lowering: every join becomes
    // nested loops. Algorithm selection (a sibling hash-match node for equi-joins),
    // the stream-vs-hash aggregate choice, and cost-based decisions are future work.
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
                    return PlanNestedLoops((LogicalJoin)node);
                case LogicalOperatorKind.Apply:
                    return PlanApply((LogicalApply)node);
                case LogicalOperatorKind.Aggregate:
                    return PlanStreamAggregates((LogicalAggregate)node);
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

        private static PhysicalOperator PlanNestedLoops(LogicalJoin node)
        {
            var left = PlanOperator(node.Left);
            var right = PlanOperator(node.Right);
            return new PhysicalNestedLoops(node.JoinKind, left, right, node.Conditions, node.Probe, node.PassthruPredicate, ImmutableArray<ValueSlot>.Empty);
        }

        // An Apply is a dependent join: it has no join condition of its own (the
        // correlation lives in the right subtree) and is executed as nested loops with
        // the referenced left columns exposed to the right as outer references.
        private static PhysicalOperator PlanApply(LogicalApply node)
        {
            var left = PlanOperator(node.Left);
            var right = PlanOperator(node.Right);
            return new PhysicalNestedLoops(MapApplyKind(node.ApplyKind), left, right, ImmutableArray<LogicalExpression>.Empty, node.Probe, passthruPredicate: null, node.OuterReferences);
        }

        private static LogicalJoinKind MapApplyKind(LogicalApplyKind kind)
        {
            return kind switch
            {
                LogicalApplyKind.Inner => LogicalJoinKind.Inner,
                LogicalApplyKind.LeftOuter => LogicalJoinKind.LeftOuter,
                LogicalApplyKind.LeftSemi => LogicalJoinKind.LeftSemi,
                LogicalApplyKind.LeftAntiSemi => LogicalJoinKind.LeftAntiSemi,
                _ => throw ExceptionBuilder.UnexpectedValue(kind)
            };
        }

        private static PhysicalOperator PlanStreamAggregates(LogicalAggregate node)
        {
            var input = PlanOperator(node.Input);

            // A stream aggregate consumes its input one group at a time, so the rows must
            // arrive grouped. Sort on the grouping columns (reusing their comparers) to
            // guarantee that. A hash aggregate, which wouldn't need this, is future work.
            if (!node.Groups.IsEmpty)
                input = new PhysicalSort(isDistinct: false, input, node.Groups);

            return new PhysicalStreamAggregates(input, node.Groups, node.Aggregates);
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
    }
}
