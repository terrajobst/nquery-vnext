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
            var other = obj as SymbolMarkupToken;
            return other is not null && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Kind * 397) ^ Text.GetHashCode();
            }
        }
    }
}