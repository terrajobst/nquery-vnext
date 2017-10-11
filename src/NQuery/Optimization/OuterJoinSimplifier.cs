using System;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class OuterJoinSimplifier : BoundTreeRewriter
    {
        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            if (node.JoinType == BoundJoinType.RightOuter)
            {
                node = node.Update(BoundJoinType.LeftOuter, node.Right, node.Left, node.Condition, node.Probe, node.PassthruPredicate);
            }

            return base.RewriteJoinRelation(node);
        }
    }
}
