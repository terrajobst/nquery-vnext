using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundOrderedQuery : BoundQuery
    {
        private readonly BoundQuery _input;
        private readonly IList<BoundOrderByColumn> _columns;

        public BoundOrderedQuery(BoundQuery input, IList<BoundOrderByColumn> columns)
        {
            _input = input;
            _columns = columns;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.OrderedQuery; }
        }

        public override ReadOnlyCollection<QueryColumnInstanceSymbol> OutputColumns
        {
            get { return _input.OutputColumns; }
        }

        public BoundQuery Input
        {
            get { return _input; }
        }

        public IList<BoundOrderByColumn> Columns
        {
            get { return _columns; }
        }
    }
}