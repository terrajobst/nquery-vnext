using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundQueryRelation : BoundRelation
    {
        private readonly BoundRelation _relation;
        private readonly ReadOnlyCollection<QueryColumnInstanceSymbol> _output;

        public BoundQueryRelation(BoundRelation relation, IList<QueryColumnInstanceSymbol> output)
        {
            _relation = relation;
            _output = new ReadOnlyCollection<QueryColumnInstanceSymbol>(output);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.QueryRelation; }
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