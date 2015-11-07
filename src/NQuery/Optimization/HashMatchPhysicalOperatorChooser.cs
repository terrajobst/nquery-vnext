using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class HashMatchPhysicalOperatorChooser : BoundTreeRewriter
    {
        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            var op = GetLogicalOperator(node.JoinType);
            if (op == null)
                return base.RewriteJoinRelation(node);

            // The optimizer already made sure that left usually contains the bigger
            // relations.
            //
            // For a hash match, the left side is the one we're indexing. Hence, it's
            // beneficial to make sure the we use the smaller relation as the left side.
            //
            // Hence, we simply swap left and right. However, this transformation is
            // only valid if it's an inner join -- for the other join types it would
            // change the semantics of the join.

            var swapLeftRight = op == BoundHashMatchOperator.Inner;

            var left = RewriteRelation(swapLeftRight ? node.Right : node.Left);
            var leftOutputs = left.GetOutputValues().ToImmutableArray();

            var right = RewriteRelation(swapLeftRight ? node.Left : node.Right);
            var rightOutputs = right.GetOutputValues().ToImmutableArray();

            var equalPredicatesInfo = GetEqualPredicatesInfo(node.Condition, leftOutputs, rightOutputs);
            if (equalPredicatesInfo.Predicates.Any())
            {
                var equalPredicate = equalPredicatesInfo.Predicates.First();
                var otherPredicates = Condition.And(equalPredicatesInfo.Predicates.Skip(1).Select(p => p.Condition));
                var remainder = Condition.And(equalPredicatesInfo.Remainder, otherPredicates);
                return new BoundHashMatchRelation(op.Value, left, right, equalPredicate.Left, equalPredicate.Right, remainder);
            }

            return base.RewriteJoinRelation(node);
        }

        private static BoundHashMatchOperator? GetLogicalOperator(BoundJoinType joinType)
        {
            switch (joinType)
            {
                case BoundJoinType.Inner:
                    return BoundHashMatchOperator.Inner;
                case BoundJoinType.FullOuter:
                    return BoundHashMatchOperator.FullOuter;
                case BoundJoinType.LeftOuter:
                    return BoundHashMatchOperator.LeftOuter;
                case BoundJoinType.RightOuter:
                    return BoundHashMatchOperator.RightOuter;
                default:
                    return null;
            }
        }

        private static EqualPredicatesInfo GetEqualPredicatesInfo(BoundExpression condition, ImmutableArray<ValueSlot> leftValues, ImmutableArray<ValueSlot> rightValues)
        {
            var conjunctions = Condition.SplitConjunctions(condition);
            var remainingConjunctions = new List<BoundExpression>();
            var equalPredicates = new List<EqualPredicate>();

            foreach (var conjunction in conjunctions)
            {
                var equalPredicate = GetEqualPredicate(conjunction, leftValues, rightValues);
                if (equalPredicate == null)
                    remainingConjunctions.Add(conjunction);
                else
                    equalPredicates.Add(equalPredicate);
            }

            var remainder = Condition.And(remainingConjunctions);
            return new EqualPredicatesInfo(remainder, equalPredicates);
        }

        private static EqualPredicate GetEqualPredicate(BoundExpression predicate, ImmutableArray<ValueSlot> left, ImmutableArray<ValueSlot> right)
        {
            var binary = predicate as BoundBinaryExpression;
            if (binary == null || binary.OperatorKind != BinaryOperatorKind.Equal)
                return null;

            var leftExpression = binary.Left as BoundValueSlotExpression;
            var rightExpression = binary.Right as BoundValueSlotExpression;
            if (leftExpression == null || rightExpression == null)
                return null;

            var source = leftExpression.ValueSlot;
            var target = rightExpression.ValueSlot;

            if (left.Contains(source) && right.Contains(target))
                return new EqualPredicate(predicate, source, target);

            if (left.Contains(target) && right.Contains(source))
                return new EqualPredicate(predicate, target, source);

            return null;
        }

        private sealed class EqualPredicatesInfo
        {
            public EqualPredicatesInfo(BoundExpression remainder, IEnumerable<EqualPredicate> equalPredicates)
            {
                Remainder = remainder;
                Predicates = equalPredicates.ToImmutableArray();
            }

            public BoundExpression Remainder { get; }

            public ImmutableArray<EqualPredicate> Predicates { get; }
        }

        private sealed class EqualPredicate
        {
            public EqualPredicate(BoundExpression condition, ValueSlot left, ValueSlot right)
            {
                Condition = condition;
                Left = left;
                Right = right;
            }

            public BoundExpression Condition { get; }

            public ValueSlot Left { get; }

            public ValueSlot Right { get; }
        }
    }
}