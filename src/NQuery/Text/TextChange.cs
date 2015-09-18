using System;

namespace NQuery.Text
{
    public struct TextChange : IEquatable<TextChange>
    {
        public static TextChange ForReplacement(TextSpan span, string newText)
        {
            return new TextChange(span, newText);
        }

        public static TextChange ForInsertion(int position, string text)
        {
            var span = new TextSpan(position, 0);
            return new TextChange(span, text);
        }

        public static TextChange ForDeletion(TextSpan span)
        {
            return new TextChange(span, string.Empty);
        }

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
            return $"[{Span.Start},{Span.End}) => {{{_newText}}}";
        }
    }
}