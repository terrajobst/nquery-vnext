using System;

namespace NQuery.Text
{
    public struct TextLine : IEquatable<TextLine>
    {
        private readonly TextBuffer _textBuffer;
        private readonly int _start;
        private readonly int _length;

        public TextLine(TextBuffer textBuffer, int start, int length)
        {
            _textBuffer = textBuffer;
            _start = start;
            _length = length;
        }

        public TextBuffer TextBuffer
        {
            get { return _textBuffer; }
        }

        public TextSpan Span
        {
            get { return new TextSpan(_start, _length); }
        }

        public TextSpan SpanWithLineBreak
        {
            get
            {
                var nextLineIndex = Index + 1;
                var nextLine = nextLineIndex < _textBuffer.Lines.Count
                                ? (TextLine?) _textBuffer.Lines[nextLineIndex]
                                : null;
                var start = Span.Start;
                var end = Span.End;
                var nextStart = nextLine == null
                                    ? end
                                    : nextLine.Value.Span.Start;
                return TextSpan.FromBounds(start, nextStart);
            }
        }

        public int Index
        {
            get { return _textBuffer.GetLineNumberFromPosition(_start); }
        }

        public string GetText()
        {
            return _textBuffer.GetText(_start, _length);
        }

        public bool Equals(TextLine other)
        {
            return _textBuffer == other._textBuffer &&
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
                var hashCode = (_textBuffer != null ? _textBuffer.GetHashCode() : 0);
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