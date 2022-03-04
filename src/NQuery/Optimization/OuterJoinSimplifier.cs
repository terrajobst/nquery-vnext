using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class OuterJoinSimplifier : BoundTreeRewriter
    {
        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            if (node.JoinType == BoundJoinType.RightOuter)
            {
                var newLeft = node.Right;
                var newRight = node.Left;
                node = node.Update(BoundJoinType.LeftOuter, newLeft, newRight, node.Condition, node.Probe, node.PassthruPredicate);
            }

            return base.RewriteJoinRelation(node);
        }
    }
}