using System;
using System.Collections.Generic;
using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.BoundNodes
{
    internal sealed class BoundLiteralExpression : BoundExpression
    {
        private readonly object _value;

        public BoundLiteralExpression(object value)
        {
            _value = value;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.LiteralExpression; }
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
            get
            {
                return _value == null
                           ? typeof(DBNull)
                           : _value.GetType();
            }
        }
    }
}