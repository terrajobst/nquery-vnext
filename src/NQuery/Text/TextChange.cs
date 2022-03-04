namespace NQuery.Text
{
    public struct TextChange : IEquatable<TextChange>
    {
        public static TextChange ForReplacement(TextSpan span, string newText)
        {
            if (newText is null)
                throw new ArgumentNullException(nameof(newText));

            return new TextChange(span, newText);
        }

        public static TextChange ForInsertion(int position, string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var span = new TextSpan(position, 0);
            return new TextChange(span, text);
        }

        public static TextChange ForDeletion(TextSpan span)
        {
            return new TextChange(span, string.Empty);
        }

        public TextChange(TextSpan span, string newText)
        {
            if (newText is null)
                throw new ArgumentNullException(nameof(newText));

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
            var other = obj as TextChange?;
            return other is not null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Span.GetHashCode() * 397) ^ (NewText is not null ? NewText.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"[{Span.Start},{Span.End}) => {{{NewText}}}";
        }
    }
}