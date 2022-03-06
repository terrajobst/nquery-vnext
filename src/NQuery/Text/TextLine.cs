namespace NQuery.Text
{
    public struct TextLine : IEquatable<TextLine>
    {
        private readonly int _start;
        private readonly int _length;

        public TextLine(SourceText text, int start, int length)
        {
            ArgumentNullException.ThrowIfNull(text);

            Text = text;
            _start = start;
            _length = length;
        }

        public SourceText Text { get; }

        public TextSpan Span
        {
            get { return new TextSpan(_start, _length); }
        }

        public TextSpan SpanIncludingLineBreak
        {
            get
            {
                var nextLineIndex = LineNumber + 1;
                var nextLine = nextLineIndex < Text.Lines.Count
                                ? (TextLine?)Text.Lines[nextLineIndex]
                                : null;
                var start = Span.Start;
                var end = Span.End;
                var nextStart = nextLine is null
                                    ? end
                                    : nextLine.Value.Span.Start;
                return TextSpan.FromBounds(start, nextStart);
            }
        }

        public int LineBreakLength
        {
            get { return SpanIncludingLineBreak.Length - Span.Length; }
        }

        public int LineNumber
        {
            get { return Text.GetLineNumberFromPosition(_start); }
        }

        public string GetText()
        {
            return Text.GetText(_start, _length);
        }

        public bool Equals(TextLine other)
        {
            return Text == other.Text &&
                   _start == other._start &&
                   _length == other._length;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TextLine?;
            return other is not null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text, _start, _length);
        }

        public static bool operator ==(TextLine left, TextLine right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TextLine left, TextLine right)
        {
            return !left.Equals(right);
        }
    }
}