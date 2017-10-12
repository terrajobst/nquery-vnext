using System.Collections.Generic;

using NQuery.Binding;

namespace NQuery.Optimization
{
    internal sealed class SemiJoinSimplifier :  BoundTreeRewriter
    {
        private Stack<bool> _semiJoinContextFlagStack = new Stack<bool>();

        public bool IsSemiJoinContext
        {
            get
            {
                if (_semiJoinContextFlagStack.Count == 0)
                    return false;

                return _semiJoinContextFlagStack.Peek();
            }
        }

        protected override BoundRelation RewriteProjectRelation(BoundProjectRelation node)
        {
            if (IsSemiJoinContext)
                return RewriteRelation(node.Input);

            return base.RewriteProjectRelation(node);
        }

        protected override BoundRelation RewriteJoinRelation(BoundJoinRelation node)
        {
            var newLeft = RewriteRelation(node.Left);

            bool semiJoinContext;
            semiJoinContext = (node.JoinType == BoundJoinType.LeftSemi ||
                               node.JoinType == BoundJoinType.LeftAntiSemi);
            _semiJoinContextFlagStack.Push(semiJoinContext);
            var newRight = RewriteRelation(node.Right);
            _semiJoinContextFlagStack.Pop();

            return node.Update(node.JoinType, newLeft, newRight, node.Condition, node.Probe, node.PassthruPredicate);
        }
    }
}