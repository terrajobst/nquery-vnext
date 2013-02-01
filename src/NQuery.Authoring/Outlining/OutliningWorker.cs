using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.Outlining
{
    internal sealed class OutliningWorker
    {
        private readonly TextBuffer _textBuffer;
        private readonly List<OutliningRegionSpan> _outlineRegions;
        private readonly TextSpan _span;

        public OutliningWorker(TextBuffer textBuffer, List<OutliningRegionSpan> outlineRegions, TextSpan span)
        {
            _textBuffer = textBuffer;
            _outlineRegions = outlineRegions;
            _span = span;
        }

        private void AddOutlineRegion(TextSpan textSpan, string text)
        {
            if (!IsMultipleLines(textSpan))
                return;

            var outliningData = new OutliningRegionSpan(textSpan, text);
            _outlineRegions.Add(outliningData);
        }

        private bool IsMultipleLines(TextSpan textSpan)
        {
            var start = _textBuffer.GetLineFromPosition(textSpan.Start);
            var end = _textBuffer.GetLineFromPosition(textSpan.End);
            return start.Index != end.Index;
        }

        public void Visit(SyntaxNode node)
        {
            if (!node.FullSpan.OverlapsWith(_span))
                return;

            switch (node.Kind)
            {
                case SyntaxKind.OrderedQuery:
                    VisitOrderedQuery((OrderedQuerySyntax) node);
                    break;
                case SyntaxKind.SelectQuery:
                    VisitSelectQuery((SelectQuerySyntax)node);
                    break;
            }

            var nodes = node.ChildNodesAndTokens()
                .SkipWhile(c => !c.FullSpan.IntersectsWith(_span))
                .TakeWhile(c => c.FullSpan.IntersectsWith(_span));

            foreach (var syntaxNodeOrToken in nodes)
                Visit(syntaxNodeOrToken);
        }

        private void VisitOrderedQuery(OrderedQuerySyntax node)
        {
            var text = node.Query is SelectQuerySyntax ? "SELECT" : "...";
            AddOutlineRegion(node.Span, text);
        }

        private void VisitSelectQuery(SelectQuerySyntax node)
        {
            var parentIsOrderedQuery = node.Parent is OrderedQuerySyntax;
            if (!parentIsOrderedQuery)
                AddOutlineRegion(node.Span, "SELECT");
        }

        private void Visit(SyntaxNodeOrToken nodeOrToken)
        {
            var asNode = nodeOrToken.AsNode();
            if (asNode != null)
                Visit(asNode);
            else
                Visit(nodeOrToken.AsToken());
        }

        private void Visit(SyntaxToken token)
        {
            foreach (var trivia in token.LeadingTrivia)
                Visit(trivia);

            foreach (var trivia in token.TrailingTrivia)
                Visit(trivia);
        }

        private void Visit(SyntaxTrivia trivia)
        {
            if (trivia.Kind == SyntaxKind.SingleLineCommentTrivia || trivia.Kind == SyntaxKind.MultiLineCommentTrivia)
                AddOutlineRegion(trivia.Span, "/**/");
        }
    }
}