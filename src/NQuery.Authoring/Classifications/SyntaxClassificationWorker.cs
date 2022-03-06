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

        private void ClassifyNodeOrToken(SyntaxNodeOrToken nodeOrToken)
        {
            var asNode = nodeOrToken.AsNode();
            if (asNode is not null)
                ClassifyNode(asNode);
            else
                ClassifyToken(nodeOrToken.AsToken());
        }

        private void ClassifyToken(SyntaxToken token)
        {
            foreach (var trivia in token.LeadingTrivia)
                ClassifyTrivia(trivia);

            var kind = GetClassificationForToken(token);
            if (kind is not null)
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
            else if (trivia.Structure is not null)
                ClassifyNode(trivia.Structure);
        }

        private static SyntaxClassification? GetClassificationForToken(SyntaxToken token)
        {
            if (token.Kind.IsKeyword())
                return SyntaxClassification.Keyword;

            if (token.Kind.IsPunctuation())
                return SyntaxClassification.Punctuation;

            return token.Kind switch
            {
                SyntaxKind.IdentifierToken => SyntaxClassification.Identifier,
                SyntaxKind.StringLiteralToken => SyntaxClassification.StringLiteral,
                SyntaxKind.NumericLiteralToken => SyntaxClassification.NumberLiteral,
                _ => null
            };
        }
    }
}