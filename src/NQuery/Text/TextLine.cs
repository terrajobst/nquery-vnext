using System;

namespace NQuery.Text
{
    public struct TextLine : IEquatable<TextLine>
    {
        private readonly SourceText _text;
        private readonly int _start;
        private readonly int _length;

        public TextLine(SourceText text, int start, int length)
        {
            _text = text;
            _start = start;
            _length = length;
        }

        public SourceText Text
        {
            get { return _text; }
        }

        public TextSpan Span
        {
            get { return new TextSpan(_start, _length); }
        }

        public TextSpan SpanIncludingLineBreak
        {
            get
            {
                var nextLineIndex = LineNumber + 1;
                var nextLine = nextLineIndex < _text.Lines.Count
                                ? (TextLine?) _text.Lines[nextLineIndex]
                                : null;
                var start = Span.Start;
                var end = Span.End;
                var nextStart = nextLine == null
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
            get { return _text.GetLineNumberFromPosition(_start); }
        }

        public string GetText()
        {
            return _text.GetText(_start, _length);
        }

        public bool Equals(TextLine other)
        {
            return _text == other._text &&
                   _start == other._start &&
                   _length == other._length;
        }

        public override bool Equals(object obj)
        {
            var other = obj as TextLine?;
            return other != null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_text != null ? _text.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ _start;
                hashCode = (hashCode*397) ^ _length;
                return hashCode;
            }
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