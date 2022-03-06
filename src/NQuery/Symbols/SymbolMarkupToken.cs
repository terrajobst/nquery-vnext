namespace NQuery.Symbols
{
    public sealed class SymbolMarkupToken : IEquatable<SymbolMarkupToken>
    {
        internal SymbolMarkupToken(SymbolMarkupKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        public SymbolMarkupKind Kind { get; }

        public string Text { get; }

        public bool Equals(SymbolMarkupToken other)
        {
            return Kind == other.Kind &&
                   string.Equals(Text, other.Text);
        }

        public override bool Equals(object obj)
        {
            return obj is SymbolMarkupToken other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Text);
        }
    }
}