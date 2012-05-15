using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language
{
    public struct SyntaxToken
    {
        private readonly SyntaxKind _kind;
        private readonly SyntaxKind _contextualKind;
        private readonly bool _isMissing;
        private readonly string _text;
        private readonly TextSpan _span;
        private readonly ReadOnlyCollection<SyntaxTrivia> _leadingTrivia;
        private readonly ReadOnlyCollection<SyntaxTrivia> _trailingTrivia;

        public SyntaxToken(SyntaxKind kind, SyntaxKind contextualKind, bool isMissing, TextSpan span, string text, IList<SyntaxTrivia> leadingTrivia, IList<SyntaxTrivia> trailingTrivia)
        {
            _kind = kind;
            _contextualKind = contextualKind;
            _isMissing = isMissing;
            _text = text;
            _span = span;
            _leadingTrivia = new ReadOnlyCollection<SyntaxTrivia>(leadingTrivia);
            _trailingTrivia = new ReadOnlyCollection<SyntaxTrivia>(trailingTrivia);
        }

        public SyntaxKind Kind
        {
            get { return _kind; }
        }

        public SyntaxKind ContextualKind
        {
            get { return _contextualKind; }
        }

        public bool IsMissing
        {
            get { return _isMissing; }
        }

        public string Text
        {
            get { return _text; }
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public TextSpan FullSpan
        {
            get
            {
                var start = _leadingTrivia.Count == 0
                                ? _span.Start
                                : _leadingTrivia[0].Span.Start;
                var end = _trailingTrivia.Count == 0
                              ? _span.End
                              : _trailingTrivia[_trailingTrivia.Count - 1].Span.End;
                return TextSpan.FromBounds(start, end);
            }
        }

        public ReadOnlyCollection<SyntaxTrivia> LeadingTrivia
        {
            get { return _leadingTrivia; }
        }

        public ReadOnlyCollection<SyntaxTrivia> TrailingTrivia
        {
            get { return _trailingTrivia; }
        }
    }
}