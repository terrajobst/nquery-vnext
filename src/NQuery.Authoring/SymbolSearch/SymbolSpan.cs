using System;

using NQuery.Text;

namespace NQuery.Authoring.SymbolSearch
{
    public struct SymbolSpan : IEquatable<SymbolSpan>
    {
        private readonly SymbolSpanKind _kind;
        private readonly Symbol _symbol;
        private readonly TextSpan _span;

        public SymbolSpan(SymbolSpanKind kind, Symbol symbol, TextSpan span)
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            _kind = kind;
            _symbol = symbol;
            _span = span;
        }

        public SymbolSpanKind Kind
        {
            get { return _kind; }
        }

        public Symbol Symbol
        {
            get { return _symbol; }
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public bool Equals(SymbolSpan other)
        {
            return _kind == other._kind &&
                   _symbol == other._symbol &&
                   _span == other._span;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SymbolSpan?;
            return other.HasValue && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) _kind;
                hashCode = (hashCode*397) ^ _symbol.GetHashCode();
                hashCode = (hashCode*397) ^ _span.GetHashCode();
                return hashCode;
            }
        }

        public static SymbolSpan CreateReference(Symbol symbol, TextSpan span)
        {
            return new SymbolSpan(SymbolSpanKind.Reference, symbol, span);
        }

        public static SymbolSpan CreateDefinition(Symbol symbol, TextSpan span)
        {
            return new SymbolSpan(SymbolSpanKind.Definition, symbol, span);
        }
    }
}