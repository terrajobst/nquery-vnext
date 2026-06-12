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
                case LogicalOperatorKind.Assert:
                    return PlanAssert((LogicalAssert)node);
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
            var concatenation = new PhysicalConcatenation(inputs, node.DefinedValues);
            if (node.IsUnionAll)
                return concatenation;

            // A plain UNION removes duplicates. Mirror the legacy lowering: a distinct
            // sort over the unified columns (with their comparers) above the concatenation.
            var sortedValues = node.DefinedValues
                                   .Zip(node.Comparers, (v, c) => new BoundComparedValue(v.ValueSlot, c))
                                   .ToImmutableArray();
            return new PhysicalSort(isDistinct: true, concatenation, sortedValues);
        }

        private static PhysicalOperator PlanIntersectOrExcept(LogicalIntersectOrExcept node)
        {
            // INTERSECT / EXCEPT lower to a distinct sort on the left feeding a semi-join
            // (intersect) or anti-semi-join (except) against the right, matching on every
            // column with NULLs treated as equal. The intersect-vs-except split is made
            // here, so there is no dedicated physical or executable set-operation node.
            var left = PlanOperator(node.Left);
            var right = PlanOperator(node.Right);

            var leftValues = left.OutputValueSlots;
            var rightValues = right.OutputValueSlots;

            var sortedValues = leftValues.Zip(node.Comparers, (v, c) => new BoundComparedValue(v, c)).ToImmutableArray();
            var distinctLeft = new PhysicalSort(isDistinct: true, left, sortedValues);

            var conditions = Enumerable.Range(0, leftValues.Length)
                                       .Select(i => BuildNullSafeEquality(leftValues[i], rightValues[i]))
                                       .ToImmutableArray();

            var joinKind = node.IsIntersect ? LogicalJoinKind.LeftSemi : LogicalJoinKind.LeftAntiSemi;

            return new PhysicalNestedLoops(joinKind, distinctLeft, right, conditions, probe: null, passthruPredicate: null, ImmutableArray<ValueSlot>.Empty);
        }

        // (l = r) OR (l IS NULL AND r IS NULL). Plain equality yields NULL when either
        // side is NULL, so two NULLs would not match; the second disjunct restores the
        // set-operation rule that NULL equals NULL. A single-NULL pair stays NULL, which
        // the nested-loops predicate compiles to a non-match.
        private static LogicalExpression BuildNullSafeEquality(ValueSlot left, ValueSlot right)
        {
            var leftExpr = new LogicalValueSlotExpression(left);
            var rightExpr = new LogicalValueSlotExpression(right);

            var equal = Binary(leftExpr, BinaryOperatorKind.Equal, rightExpr);
            var bothNull = Binary(new LogicalIsNullExpression(leftExpr), BinaryOperatorKind.LogicalAnd, new LogicalIsNullExpression(rightExpr));
            return Binary(equal, BinaryOperatorKind.LogicalOr, bothNull);
        }

        private static LogicalBinaryExpression Binary(LogicalExpression left, BinaryOperatorKind kind, LogicalExpression right)
        {
            var result = BinaryOperator.Resolve(kind, left.Type, right.Type);
            return new LogicalBinaryExpression(left, kind, result, right);
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

        private static PhysicalOperator PlanAssert(LogicalAssert node)
        {
            var input = PlanOperator(node.Input);
            return new PhysicalAssert(input, node.Condition, node.Message);
        }
    }
}
