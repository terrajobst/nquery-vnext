using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Syntax;
using NQuery.Text;

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
            VisitTriviaList(token.LeadingTrivia);
            VisitTriviaList(token.TrailingTrivia);
        }

        private void VisitTriviaList(IReadOnlyCollection<SyntaxTrivia> trivias)
        {
            FindMultilineComments(trivias);
            FindConsecutiveSingleLineComments(trivias);
        }

        private void FindMultilineComments(IEnumerable<SyntaxTrivia> trivias)
        {
            foreach (var trivia in trivias)
            {
                if (trivia.Kind == SyntaxKind.MultiLineCommentTrivia)
                {
                    var commentStart = trivia.Span.Start;
                    var line = _textBuffer.GetLineFromPosition(commentStart);
                    var firstLineEnd = line.Span.End;
                    var firstLineSpan = TextSpan.FromBounds(commentStart, firstLineEnd);
                    var text = _textBuffer.GetText(firstLineSpan) + " ...";

                    AddOutlineRegion(trivia.Span, text);
                }
            }
        }

        private void FindConsecutiveSingleLineComments(IEnumerable<SyntaxTrivia> trivias)
        {
            SyntaxTrivia firstComment = null;
            SyntaxTrivia lastComment = null;

            foreach (var trivia in trivias)
            {
                if (trivia.Kind == SyntaxKind.SingleLineCommentTrivia)
                {
                    if (firstComment == null)
                        firstComment = trivia;
                    lastComment = trivia;
                }
                else if (trivia.Kind == SyntaxKind.WhitespaceTrivia || trivia.Kind == SyntaxKind.EndOfLineTrivia)
                {
                    // Ignore
                }
                else
                {
                    if (firstComment != null)
                        AddConsecutiveSingleLineComments(firstComment, lastComment);

                    firstComment = null;
                    lastComment = null;
                }
            }

            if (firstComment != null)
                AddConsecutiveSingleLineComments(firstComment, lastComment);
        }

        private void AddConsecutiveSingleLineComments(SyntaxTrivia firstComment, SyntaxTrivia lastComment)
        {
            var start = firstComment.Span.Start;
            var end = lastComment.Span.End;
            var span = TextSpan.FromBounds(start, end);
            var text = firstComment.Text + " ...";
            AddOutlineRegion(span, text);
        }
    }
}