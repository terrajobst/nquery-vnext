#nullable enable

using System.Collections;
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
            // Nested loops can't produce a full outer join. Expanding it into operators
            // that can is a planning-time strategy choice, so it happens here rather than
            // in the algebra: the logical tree keeps the FULL OUTER as a single join.
            if (node.JoinKind == LogicalJoinKind.FullOuter)
                return PlanOperator(ExpandFullOuterJoin(node));

            var left = PlanOperator(node.Left);
            var right = PlanOperator(node.Right);
            return new PhysicalNestedLoops(MapJoinKind(node.JoinKind), left, right, node.Conditions, node.Probe, node.PassthruPredicate, ImmutableArray<ValueSlot>.Empty);
        }

        // FULL OUTER JOIN expanded into operators nested loops can run:
        //
        //   (L LEFT OUTER JOIN R ON p)
        //   UNION ALL
        //   (project (NULL-as-L, R) over (R LEFT ANTI SEMI JOIN L ON p))
        //
        // The second branch contributes the right rows with no left match, the left
        // columns padded with NULL. Each branch is an independent scan of L and R, so
        // each gets a slot-disjoint clone of the inputs (and the condition); the union
        // re-defines the join's original output slots by unifying the branches column by
        // column.
        //
        // TODO: Once a hash-match join exists, prefer it for an equi-condition full outer
        //       join -- it produces FULL OUTER directly in a single pass over each input,
        //       avoiding this expansion's double scan and clone. Fall back to this
        //       expansion only for non-equi (or subquery) conditions a hash match can't do.
        private static LogicalOperator ExpandFullOuterJoin(LogicalJoin node)
        {
            var outputs = node.OutputValueSlots;

            // Branch 1: L LEFT OUTER JOIN R -- outputs L's columns then R's.
            var cloner1 = new LogicalOperatorCloner();
            var branch1 = new LogicalJoin(
                LogicalJoinKind.LeftOuter,
                cloner1.Clone(node.Left),
                cloner1.Clone(node.Right),
                node.Conditions.Select(cloner1.CloneExpression).ToImmutableArray(),
                probe: null,
                passthruPredicate: null);

            // Branch 2: the right rows with no left match, left columns padded with NULL.
            var cloner2 = new LogicalOperatorCloner();
            var right2 = cloner2.Clone(node.Right);
            var left2 = cloner2.Clone(node.Left);
            var antiSemi = new LogicalJoin(
                LogicalJoinKind.LeftAntiSemi,
                right2,
                left2,
                node.Conditions.Select(cloner2.CloneExpression).ToImmutableArray(),
                probe: null,
                passthruPredicate: null);

            var nullSlots = left2.OutputValueSlots.Select(s => s.Duplicate()).ToImmutableArray();
            var nullValues = nullSlots.Select(s => new LogicalComputedValue(new LogicalLiteralExpression(null), s)).ToImmutableArray();
            var compute = new LogicalCompute(antiSemi, nullValues);

            // Reorder to (NULL-as-L ++ R), matching branch 1's (L ++ R) column order.
            var branch2 = new LogicalProject(compute, nullSlots.Concat(right2.OutputValueSlots).ToImmutableArray());

            var firstOutputs = branch1.OutputValueSlots;
            var secondOutputs = branch2.OutputValueSlots;
            var unifiedValues = Enumerable.Range(0, outputs.Length)
                                          .Select(i => new BoundUnifiedValue(outputs[i], new[] { firstOutputs[i], secondOutputs[i] }))
                                          .ToImmutableArray();

            return new LogicalUnion(isUnionAll: true, ImmutableArray.Create<LogicalOperator>(branch1, branch2), unifiedValues, ImmutableArray<IComparer>.Empty);
        }

        // A full outer join has no PhysicalJoinKind: nested loops can't produce it.
        // PlanNestedLoops expands it (into left-outer UNION ALL right-anti-semi) before
        // mapping, so it shouldn't reach here; this is a defensive guard in case a
        // LogicalJoin with that kind reaches MapJoinKind by another path.
        private static PhysicalJoinKind MapJoinKind(LogicalJoinKind kind)
        {
            return kind switch
            {
                LogicalJoinKind.Inner => PhysicalJoinKind.Inner,
                LogicalJoinKind.LeftOuter => PhysicalJoinKind.LeftOuter,
                LogicalJoinKind.LeftSemi => PhysicalJoinKind.LeftSemi,
                LogicalJoinKind.LeftAntiSemi => PhysicalJoinKind.LeftAntiSemi,
                LogicalJoinKind.FullOuter => throw new NotSupportedException("FULL OUTER JOIN requires a hash match, which is not yet implemented."),
                _ => throw ExceptionBuilder.UnexpectedValue(kind)
            };
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

        private static PhysicalJoinKind MapApplyKind(LogicalApplyKind kind)
        {
            return kind switch
            {
                LogicalApplyKind.Inner => PhysicalJoinKind.Inner,
                LogicalApplyKind.LeftOuter => PhysicalJoinKind.LeftOuter,
                LogicalApplyKind.LeftSemi => PhysicalJoinKind.LeftSemi,
                LogicalApplyKind.LeftAntiSemi => PhysicalJoinKind.LeftAntiSemi,
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

            var joinKind = node.IsIntersect ? PhysicalJoinKind.LeftSemi : PhysicalJoinKind.LeftAntiSemi;

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
