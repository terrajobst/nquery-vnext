using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Optimization
{
    // The purpose of this rewriter is to replace FULL OUTER JOINS with
    // a LEFT SEMI JOIN and LEFT ANTI SEMI JOIN:
    //
    //      SELECT  *
    //      FROM    Table1 t1
    //                  FULL OUTER JOIN Table2 t2 ON Condition
    //
    // Concat
    //     LeftOuterJoin Condition
    //         Table1
    //         Table2
    //     Compute (Table1 = NULL, Table2)
    //         LeftAntiSemiJoin Condition
    //             Table2
    //             Table1
    //
    // The reason we expand the joins so that we can do the following:
    //
    //   * Expand subqueries
    //   * Implement the join using nested loops
    //
    // The only case where we don't want to expand the FULL OUTER JOIN
    // is if the join condition doesn't contain a subquery but has at
    // least one conjunction that can be used for a hash match.
    //
    internal sealed class FullOuterJoinExpander : BoundTreeRewriter
    {
        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            node = (BoundJoinRelation)base.RewriteJoinRelation(node);

            var needsRewriting = NeedsRewriting(node);
            if (!needsRewriting)
                return node;

            var node1 = (BoundJoinRelation)Instantiator.Instantiate(node);
            var node2 = (BoundJoinRelation)Instantiator.Instantiate(node);

            var leftOuterJoin = node1.Update(BoundJoinType.LeftOuter, node1.Left, node1.Right, node1.Condition, null, null);
            var leftAntiSemiJoin = node1.Update(BoundJoinType.LeftAntiSemi, node2.Right, node2.Left, node2.Condition, null, null);

            var computedValueSlots = node.Left.GetOutputValues().Select(v => v.Duplicate()).ToImmutableArray();
            var computedValues = computedValueSlots.Select(v => new BoundComputedValue(Expression.Null(), v));
            var compute = new BoundComputeRelation(leftAntiSemiJoin, computedValues);
            var project = new BoundProjectRelation(compute, computedValueSlots.Concat(leftAntiSemiJoin.GetOutputValues()));

            var concatValueSlots = node.GetOutputValues().ToImmutableArray();
            var firstOutputs = leftOuterJoin.GetOutputValues().ToImmutableArray();
            var secondOutputs = project.GetOutputValues().ToImmutableArray();
            var unifiedValues = new BoundUnifiedValue[concatValueSlots.Length];

            for (var i = 0; i < unifiedValues.Length; i++)
                unifiedValues[i] = new BoundUnifiedValue(concatValueSlots[i], new[] { firstOutputs[i], secondOutputs[i] });

            return new BoundConcatenationRelation(new BoundRelation[] { leftOuterJoin, project }, unifiedValues);
        }

        private static bool NeedsRewriting(BoundJoinRelation node)
        {
            if (node.JoinType != BoundJoinType.FullOuter)
                return false;

            if (SubqueryChecker.ContainsSubquery(node.Condition))
                return true;

            var left = node.Left.GetOutputValues().ToImmutableArray();
            var right = node.Right.GetOutputValues().ToImmutableArray();

            var conjunctions = Expression.SplitConjunctions(node.Condition);
            return !conjunctions.Any(c => CanBeUsedForHashMatch(left, right, c));
        }

        private static bool CanBeUsedForHashMatch(ImmutableArray<ValueSlot> left, ImmutableArray<ValueSlot> right, BoundExpression condition)
        {
            if (condition is not BoundBinaryExpression binary || binary.OperatorKind != BinaryOperatorKind.Equal)
                return false;

            var leftSlot = (binary.Left as BoundValueSlotExpression)?.ValueSlot;
            var rightSlot = (binary.Right as BoundValueSlotExpression)?.ValueSlot;
            if (leftSlot is null || rightSlot is null)
                return false;

            return left.Contains(leftSlot) && right.Contains(rightSlot) ||
                   left.Contains(rightSlot) && right.Contains(leftSlot);
        }
    }
}