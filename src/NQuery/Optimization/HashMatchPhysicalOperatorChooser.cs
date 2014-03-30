using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Binding;
using NQuery.Iterators;

namespace NQuery.Optimization
{
    internal sealed class HashMatchPhysicalOperatorChooser : BoundTreeRewriter
    {
        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            var op = GetLogicalOperator(node.JoinType);
            if (op == null)
                return base.RewriteJoinRelation(node);

            var left = RewriteRelation(node.Left);
            var leftOutputs = left.GetOutputValues().ToImmutableArray();

            var right = RewriteRelation(node.Right);
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
            if (binary == null)
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
            private readonly BoundExpression _remainder;
            private readonly ImmutableArray<EqualPredicate> _predicates;

            public EqualPredicatesInfo(BoundExpression remainder, IEnumerable<EqualPredicate> equalPredicates)
            {
                _remainder = remainder;
                _predicates = equalPredicates.ToImmutableArray();
            }

            public BoundExpression Remainder
            {
                get { return _remainder; }
            }

            public ImmutableArray<EqualPredicate> Predicates
            {
                get { return _predicates; }
            }
        }

        private sealed class EqualPredicate
        {
            private readonly BoundExpression _condition;
            private readonly ValueSlot _left;
            private readonly ValueSlot _right;

            public EqualPredicate(BoundExpression condition, ValueSlot left, ValueSlot right)
            {
                _condition = condition;
                _left = left;
                _right = right;
            }

            public BoundExpression Condition
            {
                get { return _condition; }
            }

            public ValueSlot Left
            {
                get { return _left; }
            }

            public ValueSlot Right
            {
                get { return _right; }
            }
        }
    }
}