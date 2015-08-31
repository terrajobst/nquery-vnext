using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.Classifications
{
    internal sealed class SyntaxClassificationWorker
    {
        private readonly List<SyntaxClassificationSpan> _classificationSpans;
        private readonly TextSpan _span;

        public SyntaxClassificationWorker(List<SyntaxClassificationSpan> classificationSpans, TextSpan span)
        {
            _classificationSpans = classificationSpans;
            _span = span;
        }

        private void AddClassification(TextSpan textSpan, SyntaxClassification kind)
        {
            if (textSpan.Length > 0)
                _classificationSpans.Add(new SyntaxClassificationSpan(textSpan, kind));
        }

        private void AddClassification(SyntaxToken token, SyntaxClassification kind)
        {
            AddClassification(token.Span, kind);
        }

        private void AddClassification(SyntaxTrivia trivia, SyntaxClassification kind)
        {
            AddClassification(trivia.Span, kind);
        }

        public void ClassifyNode(SyntaxNode node)
        {
            if (!node.FullSpan.OverlapsWith(_span))
                return;

            var nodes = node.ChildNodesAndTokens()
                            .SkipWhile(n => !n.FullSpan.IntersectsWith(_span))
                            .TakeWhile(n => n.FullSpan.IntersectsWith(_span));

            foreach (var syntaxNodeOrToken in nodes)
                ClassifyNodeOrToken(syntaxNodeOrToken);
        }

        public void ClassifyTokens(IEnumerable<SyntaxToken> tokens)
        {
            foreach (var token in tokens)
                ClassifyToken(token);
        }

        private void ClassifyNodeOrToken(SyntaxNodeOrToken nodeOrToken)
        {
            var asNode = nodeOrToken.AsNode();
            if (asNode != null)
                ClassifyNode(asNode);
            else
                ClassifyToken(nodeOrToken.AsToken());
        }

        private void ClassifyToken(SyntaxToken token)
        {
            foreach (var trivia in token.LeadingTrivia)
                ClassifyTrivia(trivia);

            var kind = GetClassificationForToken(token);
            if (kind != null)
                AddClassification(token, kind.Value);

            foreach (var trivia in token.TrailingTrivia)
                ClassifyTrivia(trivia);
        }

        private void ClassifyTrivia(SyntaxTrivia trivia)
        {
            if (trivia.Kind == SyntaxKind.WhitespaceTrivia || trivia.Kind == SyntaxKind.EndOfLineTrivia)
                AddClassification(trivia, SyntaxClassification.Whitespace);
            else if (trivia.Kind.IsComment())
                AddClassification(trivia, SyntaxClassification.Comment);
            else if (trivia.Structure != null)
                ClassifyNode(trivia.Structure);
        }

        private static SyntaxClassification? GetClassificationForToken(SyntaxToken token)
        {
            if (token.Kind.IsKeyword())
                return SyntaxClassification.Keyword;

            if (token.Kind.IsPunctuation())
                return SyntaxClassification.Punctuation;

            if (token.Kind == SyntaxKind.IdentifierToken)
                return SyntaxClassification.Identifier;

            if ((token.Kind == SyntaxKind.StringLiteralToken))
                return SyntaxClassification.StringLiteral;

            if (token.Kind == SyntaxKind.NumericLiteralToken)
                return SyntaxClassification.NumberLiteral;

            return null;
        }
    }
}