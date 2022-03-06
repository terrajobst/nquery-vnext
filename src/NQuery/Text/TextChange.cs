namespace NQuery.Text
{
    public struct TextChange : IEquatable<TextChange>
    {
        public static TextChange ForReplacement(TextSpan span, string newText)
        {
            ArgumentNullException.ThrowIfNull(newText);

            return new TextChange(span, newText);
        }

        public static TextChange ForInsertion(int position, string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            var span = new TextSpan(position, 0);
            return new TextChange(span, text);
        }

        public static TextChange ForDeletion(TextSpan span)
        {
            return new TextChange(span, string.Empty);
        }

        public TextChange(TextSpan span, string newText)
        {
            ArgumentNullException.ThrowIfNull(newText);

            Span = span;
            NewText = newText;
        }

        public TextSpan Span { get; }

        public string NewText { get; }

        public bool Equals(TextChange other)
        {
            return Span.Equals(other.Span) && string.Equals(NewText, other.NewText);
        }

        public override bool Equals(object obj)
        {
            return obj is TextChange other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Span, NewText);
        }

        public override string ToString()
        {
            return $"[{Span.Start},{Span.End}) => {{{NewText}}}";
        }
    }
}