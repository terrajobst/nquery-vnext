using System.Collections.ObjectModel;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundCombinedQuery : BoundQuery
    {
        private readonly BoundQuery _left;
        private readonly BoundQueryCombinator _combinator;
        private readonly BoundQuery _right;

        public BoundCombinedQuery(BoundQuery left, BoundQueryCombinator combinator, BoundQuery right)
        {
            _left = left;
            _combinator = combinator;
            _right = right;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CombinedQuery; }
        }

        public BoundQuery Left
        {
            get { return _left; }
        }

        public BoundQueryCombinator Combinator
        {
            get { return _combinator; }
        }

        public BoundQuery Right
        {
            get { return _right; }
        }

        public override ReadOnlyCollection<BoundSelectColumn> SelectColumns
        {
            get { return _left.SelectColumns; }
        }
    }
}