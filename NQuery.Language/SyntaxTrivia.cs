using System;

namespace NQuery.Language
{
    public struct SyntaxTrivia
    {
        private readonly SyntaxKind _kind;
        private readonly string _text;
        private readonly TextSpan _span;

        public SyntaxTrivia(SyntaxKind kind, string text, TextSpan span)
        {
            _kind = kind;
            _text = text;
            _span = span;
        }

        public SyntaxKind Kind
        {
            get { return _kind; }
        }

        public string Text
        {
            get { return _text; }
        }

        public TextSpan Span
        {
            get { return _span; }
        }
    }
}