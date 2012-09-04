using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Binding;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundSingleRowSubselect : BoundExpression
    {
        private readonly BoundQuery _boundQuery;
        private readonly Type _type;

        public BoundSingleRowSubselect(BoundQuery boundQuery)
        {
            _boundQuery = boundQuery;
            var firstColumn = _boundQuery.SelectColumns.FirstOrDefault();
            _type = firstColumn == null
                        ? WellKnownTypes.Unknown
                        : firstColumn.Expression.Type;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.SingleRowSubselect; }
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