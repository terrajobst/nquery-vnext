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
            if (rightSide is null || rightSide.Probe is not null || rightSide.PassthruPredicate is not null)
                return base.RewriteJoinRelation(node);

            var leftSideLeft = RewriteRelation(node.Left);
            var leftSideRight = rightSide.Left;
            var left = new BoundJoinRelation(node.JoinType, leftSideLeft, leftSideRight, null, null, null);
            var right = rightSide.Right;
            var condition = Expression.And(node.Condition, rightSide.Condition);
            var result = new BoundJoinRelation(rightSide.JoinType, left, right, condition, null, null);

            return RewriteRelation(result);
        }
    }
}