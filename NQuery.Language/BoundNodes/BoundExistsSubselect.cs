using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundExistsSubselect : BoundExpression
    {
        private readonly BoundQuery _boundQuery;

        public BoundExistsSubselect(BoundQuery boundQuery)
        {
            _boundQuery = boundQuery;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.ExistsSubselect; }
        }

        public BoundQuery BoundQuery
        {
            get { return _boundQuery; }
        }

        public override Symbol Symbol
        {
            get { return null; }
        }

        public override IEnumerable<Symbol> Candidates
        {
            get { return Enumerable.Empty<Symbol>(); }
        }

        public override Type Type
        {
            get { return typeof(bool); }
        }
    }
}