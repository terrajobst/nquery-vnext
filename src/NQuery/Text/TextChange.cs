using System;

namespace NQuery.Text
{
    public struct TextChange : IEquatable<TextChange>
    {
        private readonly TextSpan _span;
        private readonly string _newText;

        public TextChange(TextSpan span, string newText)
        {
            _span = span;
            _newText = newText;
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public string NewText
        {
            get { return _newText; }
        }

        public bool Equals(TextChange other)
        {
            return _span.Equals(other._span) && string.Equals(_newText, other._newText);
        }

        public override bool Equals(object obj)
        {
            var other = obj as TextChange?;
            return other != null && Equals(other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_span.GetHashCode()*397) ^ (_newText != null ? _newText.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}) => {{{2}}}", Span.Start, Span.End, _newText);
        }
    }
}