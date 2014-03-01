using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundQuery : BoundNode
    {
        private readonly BoundRelation _relation;
        private readonly ReadOnlyCollection<QueryColumnInstanceSymbol> _output;

        public BoundQuery(BoundRelation relation, IList<QueryColumnInstanceSymbol> output)
        {
            _relation = relation;
            _output = new ReadOnlyCollection<QueryColumnInstanceSymbol>(output);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.Query; }
        }

        public ReadOnlyCollection<QueryColumnInstanceSymbol> OutputColumns
        {
            get { return _output; }
        }

        public BoundRelation Relation
        {
            get { return _relation; }
        }
    }
}