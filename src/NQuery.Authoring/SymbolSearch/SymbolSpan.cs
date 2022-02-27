using NQuery.Text;

namespace NQuery.Authoring.SymbolSearch
{
    public struct SymbolSpan : IEquatable<SymbolSpan>
    {
        public SymbolSpan(SymbolSpanKind kind, Symbol symbol, TextSpan span)
        {
            if (symbol is null)
                throw new ArgumentNullException(nameof(symbol));

            Kind = kind;
            Symbol = symbol;
            Span = span;
        }

        public SymbolSpanKind Kind { get; }

        public Symbol Symbol { get; }

        public TextSpan Span { get; }

        public bool Equals(SymbolSpan other)
        {
            return Kind == other.Kind &&
                   Symbol == other.Symbol &&
                   Span == other.Span;
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
                var hashCode = (int) Kind;
                hashCode = (hashCode*397) ^ Symbol.GetHashCode();
                hashCode = (hashCode*397) ^ Span.GetHashCode();
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