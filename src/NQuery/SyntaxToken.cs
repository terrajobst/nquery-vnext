#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;

using NQuery.Text;

namespace NQuery
{
    public sealed class SyntaxToken
    {
        private readonly SyntaxTree? _syntaxTree;
        private readonly string? _text;

        internal SyntaxToken(SyntaxTree? syntaxTree, SyntaxKind kind, SyntaxKind contextualKind, bool isMissing, TextSpan span, string? text, object? value, IEnumerable<SyntaxTrivia> leadingTrivia, IEnumerable<SyntaxTrivia> trailingTrivia, IEnumerable<Diagnostic> diagnostics)
        {
            _syntaxTree = syntaxTree;
            Kind = kind;
            ContextualKind = contextualKind;
            IsMissing = isMissing;
            Span = span;
            _text = text;
            Value = value;
            LeadingTrivia = leadingTrivia.ToImmutableArray();
            TrailingTrivia = trailingTrivia.ToImmutableArray();
            Diagnostics = diagnostics.ToImmutableArray();
        }

        public SyntaxNode Parent => _syntaxTree?.GetParentNode(this)!;

        public SyntaxKind Kind { get; }

        public SyntaxKind ContextualKind { get; }

        public bool IsMissing { get; }

        public string Text => _text ?? Kind.GetText();

        public object? Value { get; }

        public string ValueText => Value as string ?? Text;

        public TextSpan Span { get; }

        public TextSpan FullSpan
        {
            get
            {
                var start = LeadingTrivia.Length == 0
                                ? Span.Start
                                : LeadingTrivia[0].Span.Start;
                var end = TrailingTrivia.Length == 0
                              ? Span.End
                              : TrailingTrivia[TrailingTrivia.Length - 1].Span.End;
                return TextSpan.FromBounds(start, end);
            }
        }

        public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }

        public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public SyntaxToken? GetPreviousToken(bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxTreeNavigation.GetPreviousToken(this, includeZeroLength, includeSkippedTokens);
        }

        public SyntaxToken? GetNextToken(bool includeZeroLength = false, bool includeSkippedTokens = false)
        {
            return SyntaxTreeNavigation.GetNextToken(this, includeZeroLength, includeSkippedTokens);
        }

        public void WriteTo(TextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            foreach (var syntaxTrivia in LeadingTrivia)
                syntaxTrivia.WriteTo(writer);

            writer.Write(_text);

            foreach (var syntaxTrivia in TrailingTrivia)
                syntaxTrivia.WriteTo(writer);
        }

        public bool IsEquivalentTo(SyntaxToken other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            return SyntaxTreeEquivalence.AreEquivalent(this, other);
        }

        public SyntaxToken WithDiagnotics(IEnumerable<Diagnostic> diagnostics)
        {
            if (diagnostics == null)
                throw new ArgumentNullException(nameof(diagnostics));

            return new SyntaxToken(_syntaxTree, Kind, ContextualKind, IsMissing, Span, _text, Value, LeadingTrivia, TrailingTrivia, diagnostics);
        }

        public SyntaxToken WithKind(SyntaxKind kind)
        {
            return new SyntaxToken(_syntaxTree, kind, ContextualKind, IsMissing, Span, _text, Value, LeadingTrivia, TrailingTrivia, Diagnostics);
        }

        public SyntaxToken WithLeadingTrivia(IEnumerable<SyntaxTrivia> trivia)
        {
            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return new SyntaxToken(_syntaxTree, Kind, ContextualKind, IsMissing, Span, _text, Value, trivia, TrailingTrivia, Diagnostics);
        }

        public SyntaxToken WithTrailingTrivia(IEnumerable<SyntaxTrivia> trivia)
        {
            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return new SyntaxToken(_syntaxTree, Kind, ContextualKind, IsMissing, Span, _text, Value, LeadingTrivia, trivia, Diagnostics);
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