using System;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class JoinLinearizer : BoundTreeRewriter
    {
        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            // A <IJ> (B <J> C) --> (A <IJ> B) <J> C

            if (node.JoinType != BoundJoinType.Inner)
                return base.RewriteJoinRelation(node);

            var rightSide = node.Right as BoundJoinRelation;
            if (rightSide == null)
                return base.RewriteJoinRelation(node);

            var leftSideLeft = RewriteRelation(node.Left);
            var leftSideRight = rightSide.Left;
            var left = new BoundJoinRelation(node.JoinType, leftSideLeft, leftSideRight, null);
            var right = rightSide.Right;
            var condition = Condition.And(node.Condition, rightSide.Condition);
            var result = new BoundJoinRelation(rightSide.JoinType, left, right, condition);

            return RewriteRelation(result);
        }
    }
}