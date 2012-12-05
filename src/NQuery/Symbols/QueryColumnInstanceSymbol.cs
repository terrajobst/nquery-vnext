using System;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class QueryColumnInstanceSymbol : ColumnInstanceSymbol
    {
        private readonly ExpressionSyntax _syntax;
        private readonly ValueSlot _valueSlot;

        internal QueryColumnInstanceSymbol(string name, ExpressionSyntax syntax, ValueSlot valueSlot)
            : base(name)
        {
            _syntax = syntax;
            _valueSlot = valueSlot;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.QueryColumnInstance; }
        }

        internal override ValueSlot ValueSlot
        {
            get { return _valueSlot; }
        }

        public ExpressionSyntax Syntax
        {
            get { return _syntax; }
        }
    }
}