using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

using NQuery.Text;

namespace NQuery
{
    public sealed class SyntaxToken
    {
        private readonly SyntaxTree _syntaxTree;
        private readonly SyntaxKind _kind;
        private readonly SyntaxKind _contextualKind;
        private readonly bool _isMissing;
        private readonly TextSpan _span;
        private readonly string _text;
        private readonly object _value;
        private readonly ImmutableArray<SyntaxTrivia> _leadingTrivia;
        private readonly ImmutableArray<SyntaxTrivia> _trailingTrivia;
        private readonly ImmutableArray<Diagnostic> _diagnostics;

        internal SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, SyntaxKind contextualKind, bool isMissing, TextSpan span, string text, object value, IEnumerable<SyntaxTrivia> leadingTrivia, IEnumerable<SyntaxTrivia> trailingTrivia, IEnumerable<Diagnostic> diagnostics)
        {
            _syntaxTree = syntaxTree;
            _kind = kind;
            _contextualKind = contextualKind;
            _isMissing = isMissing;
            _span = span;
            _text = text;
            _value = value;
            _leadingTrivia = leadingTrivia.ToImmutableArray();
            _trailingTrivia = trailingTrivia.ToImmutableArray();
            _diagnostics = diagnostics.ToImmutableArray();
        }

        public SyntaxNode Parent
        {
            get { return _syntaxTree == null ? null : _syntaxTree.GetParentNode(this); }
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
            get { return _text ?? _kind.GetText(); }
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
                var start = _leadingTrivia.Length == 0
                                ? _span.Start
                                : _leadingTrivia[0].Span.Start;
                var end = _trailingTrivia.Length == 0
                              ? _span.End
                              : _trailingTrivia[_trailingTrivia.Length - 1].Span.End;
                return TextSpan.FromBounds(start, end);
            }
        }

        public ImmutableArray<SyntaxTrivia> LeadingTrivia
        {
            get { return _leadingTrivia; }
        }

        public ImmutableArray<SyntaxTrivia> TrailingTrivia
        {
            get { return _trailingTrivia; }
        }

        public ImmutableArray<Diagnostic> Diagnostics
        {
            get { return _diagnostics; }
        }

        public SyntaxToken GetPreviousToken(bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxTreeNavigation.GetPreviousToken(this, includeZeroLength, includeSkippedTokens);
        }

        public SyntaxToken GetNextToken(bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxTreeNavigation.GetNextToken(this, includeZeroLength, includeSkippedTokens);
        }

        public void WriteTo(TextWriter writer)
        {
            foreach (var syntaxTrivia in LeadingTrivia)
                syntaxTrivia.WriteTo(writer);

            writer.Write(_text);

            foreach (var syntaxTrivia in TrailingTrivia)
                syntaxTrivia.WriteTo(writer);
        }

        public bool IsEquivalentTo(SyntaxToken other)
        {
            return SyntaxTreeEquivalence.AreEquivalent(this, other);
        }

        public SyntaxToken WithDiagnotics(IEnumerable<Diagnostic> diagnostics)
        {
            return new SyntaxToken(_syntaxTree, _kind, _contextualKind, _isMissing, _span, _text, _value, _leadingTrivia, _trailingTrivia, diagnostics);
        }

        public SyntaxToken WithKind(SyntaxKind kind)
        {
            return new SyntaxToken(_syntaxTree, kind, _contextualKind, _isMissing, _span, _text, _value, _leadingTrivia, _trailingTrivia, _diagnostics);
        }

        public SyntaxToken WithLeadingTrivia(IEnumerable<SyntaxTrivia> trivia)
        {
            return new SyntaxToken(_syntaxTree, _kind, _contextualKind, _isMissing, _span, _text, _value, trivia, _trailingTrivia, _diagnostics);
        }

        public SyntaxToken WithTrailingTrivia(IEnumerable<SyntaxTrivia> trivia)
        {
            return new SyntaxToken(_syntaxTree, _kind, _contextualKind, _isMissing, _span, _text, _value, _leadingTrivia, trivia, _diagnostics);
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