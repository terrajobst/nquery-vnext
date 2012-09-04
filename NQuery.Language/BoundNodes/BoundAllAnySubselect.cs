using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Binding;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundAllAnySubselect : BoundExpression
    {
        private readonly BoundQuery _boundQuery;
        private readonly Type _type;

        public BoundAllAnySubselect(BoundQuery boundQuery)
        {
            _boundQuery = boundQuery;
            var firstColumn = boundQuery.SelectColumns.FirstOrDefault();
            _type = firstColumn == null
                        ? WellKnownTypes.Unknown
                        : firstColumn.Expression.Type;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.AllAnySubselect; }
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
            get { return _type; }
        }
    }
}