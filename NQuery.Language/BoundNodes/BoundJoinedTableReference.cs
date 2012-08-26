using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundJoinedTableReference : BoundTableReference
    {
        private readonly BoundTableReference _left;
        private readonly BoundTableReference _right;
        private readonly BoundJoinType _joinType;
        private readonly BoundExpression _condition;

        public BoundJoinedTableReference(BoundJoinType joinType, BoundTableReference left, BoundTableReference right, BoundExpression condition)
        {
            _left = left;
            _right = right;
            _joinType = joinType;
            _condition = condition;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.JoinedTableReference; }
        }

        public BoundTableReference Left
        {
            get { return _left; }
        }

        public BoundTableReference Right
        {
            get { return _right; }
        }

        public BoundJoinType JoinType
        {
            get { return _joinType; }
        }

        public BoundExpression Condition
        {
            get { return _condition; }
        }

        public override IEnumerable<TableInstanceSymbol> GetDeclaredTableInstances()
        {
            return _left.GetDeclaredTableInstances().Concat(_right.GetDeclaredTableInstances());
        }
    }
}