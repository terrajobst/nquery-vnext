using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundCombinedQuery : BoundQuery
    {
        private readonly ReadOnlyCollection<QueryColumnInstanceSymbol> _outputColumns;
        private readonly BoundQuery _left;
        private readonly BoundQueryCombinator _combinator;
        private readonly BoundQuery _right;

        public BoundCombinedQuery(BoundQuery left, BoundQueryCombinator combinator, BoundQuery right, IList<QueryColumnInstanceSymbol> columns)
        {
            _outputColumns = new ReadOnlyCollection<QueryColumnInstanceSymbol>(columns);
            _left = left;
            _combinator = combinator;
            _right = right;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.CombinedQuery; }
        }

        public override ReadOnlyCollection<QueryColumnInstanceSymbol> OutputColumns
        {
            get { return _outputColumns; }
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
    }
}