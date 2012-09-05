using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace NQuery.Language
{
    public struct SyntaxToken
    {
        private readonly SyntaxNode _parent;
        private readonly SyntaxKind _kind;
        private readonly SyntaxKind _contextualKind;
        private readonly bool _isMissing;
        private readonly TextSpan _span;
        private readonly string _text;
        private readonly object _value;
        private readonly ReadOnlyCollection<SyntaxTrivia> _leadingTrivia;
        private readonly ReadOnlyCollection<SyntaxTrivia> _trailingTrivia;
        private readonly ReadOnlyCollection<Diagnostic> _diagnostics;

        public SyntaxToken(SyntaxKind kind, SyntaxKind contextualKind, bool isMissing, TextSpan span, string text, object value, IList<SyntaxTrivia> leadingTrivia, IList<SyntaxTrivia> trailingTrivia, IList<Diagnostic> diagnostics)
            : this(null, kind, contextualKind, isMissing, span, text, value, leadingTrivia, trailingTrivia, diagnostics)
        {
        }

        private SyntaxToken(SyntaxNode parent, SyntaxKind kind, SyntaxKind contextualKind, bool isMissing, TextSpan span, string text, object value, IList<SyntaxTrivia> leadingTrivia, IList<SyntaxTrivia> trailingTrivia, IList<Diagnostic> diagnostics)
        {
            _parent = parent;
            _kind = kind;
            _contextualKind = contextualKind;
            _isMissing = isMissing;
            _span = span;
            _text = text;
            _value = value;
            _leadingTrivia = new ReadOnlyCollection<SyntaxTrivia>(leadingTrivia);
            _trailingTrivia = new ReadOnlyCollection<SyntaxTrivia>(trailingTrivia);
            _diagnostics = new ReadOnlyCollection<Diagnostic>(diagnostics);
        }

        public SyntaxNode Parent
        {
            get { return _parent; }
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
            get { return _text ?? SyntaxFacts.GetText(_kind); }
        }

        public object Value
        {
            get { return _value; }
        }

        public string ValueText
        {
            get { return _value as string ?? Text; }
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

        public ReadOnlyCollection<Diagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }

        public void WriteTo(TextWriter writer)
        {
            foreach (var syntaxTrivia in LeadingTrivia)
                syntaxTrivia.WriteTo(writer);

            writer.Write(_text);

            foreach (var syntaxTrivia in TrailingTrivia)
                syntaxTrivia.WriteTo(writer);
        }

        public SyntaxToken WithParent(SyntaxNode parent)
        {
            return new SyntaxToken(parent, _kind, _contextualKind, _isMissing, _span, _text, _value, _leadingTrivia, _trailingTrivia, _diagnostics);
        }

        public SyntaxToken WithDiagnotics(IList<Diagnostic> diagnostics)
        {
            return new SyntaxToken(_parent, _kind, _contextualKind, _isMissing, _span, _text, _value, _leadingTrivia, _trailingTrivia, diagnostics);
        }

        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                WriteTo(writer);
                return writer.ToString();
            }
        }
    }
}