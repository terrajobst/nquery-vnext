using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundQuery : BoundNode
    {
        private readonly BoundRelation _relation;
        private readonly ImmutableArray<QueryColumnInstanceSymbol> _output;

        public BoundQuery(BoundRelation relation, IEnumerable<QueryColumnInstanceSymbol> output)
        {
            _relation = relation;
            _output = output.ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.Query; }
        }

        public ImmutableArray<QueryColumnInstanceSymbol> OutputColumns
        {
            get { return _output; }
        }

        public BoundRelation Relation
        {
            get { return _relation; }
        }
    }
}