using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Language;

namespace NQueryViewer.EditorIntegration
{
    internal sealed class NQueryOutliningTagger : ITagger<IOutliningRegionTag> 
    {
        private readonly ITextBuffer _textBuffer;
        private readonly INQuerySyntaxTreeManager _syntaxTreeManager;

        public NQueryOutliningTagger(ITextBuffer textBuffer, INQuerySyntaxTreeManager syntaxTreeManager)
        {
            _textBuffer = textBuffer;
            _syntaxTreeManager = syntaxTreeManager;
            _syntaxTreeManager.SyntaxTreeChanged += SyntaxTreeManagerOnSyntaxTreeChanged;
        }

        private void SyntaxTreeManagerOnSyntaxTreeChanged(object sender, EventArgs e)
        {
            var snapshot = _textBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, 0, snapshot.Length);
            OnTagsChanged(new SnapshotSpanEventArgs(snapshotSpan));
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var result = new List<ITagSpan<IOutliningRegionTag>>();
            var syntaxTree = _syntaxTreeManager.SyntaxTree;
            if (spans.Count > 0 && syntaxTree != null)
            {
                var first = spans.First();
                var last = spans.Last();
                var snapshotSpan = new SnapshotSpan(first.Start, last.End);
                var visitor = new Visitor(syntaxTree, snapshotSpan, result);
                visitor.Visit(syntaxTree.Root);
            }

            return result;
        }

        private void OnTagsChanged(SnapshotSpanEventArgs e)
        {
            var handler = TagsChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private sealed class Visitor
        {
            private readonly SyntaxTree _syntaxTree;
            private readonly ITextSnapshot _snapshot;
            private readonly TextSpan _span;
            private readonly List<ITagSpan<IOutliningRegionTag>> _outliningRegionTags;

            public Visitor(SyntaxTree syntaxTree, SnapshotSpan snapshotSpan, List<ITagSpan<IOutliningRegionTag>> outliningRegionTags)
            {
                _syntaxTree = syntaxTree;
                _snapshot = snapshotSpan.Snapshot;
                _outliningRegionTags = outliningRegionTags;
                _span = new TextSpan(snapshotSpan.Start, snapshotSpan.Length);
            }

            private void AddOutliningRegionTag(TextSpan textSpan, IOutliningRegionTag tag)
            {
                var span = new Span(textSpan.Start, textSpan.Length);
                var snapshotSpan = new SnapshotSpan(_snapshot, span);
                var tagSpan = new TagSpan<IOutliningRegionTag>(snapshotSpan, tag);
                _outliningRegionTags.Add(tagSpan);
            }

            private bool IsMultipleLines(TextSpan textSpan)
            {
                var start = _syntaxTree.TextBuffer.GetLineFromPosition(textSpan.Start);
                var end = _syntaxTree.TextBuffer.GetLineFromPosition(textSpan.End);
                return start.Index != end.Index;
            }

            public void Visit(SyntaxNode node)
            {
                if (!node.FullSpan.OverlapsWith(_span))
                    return;

                if (node.Kind == SyntaxKind.SelectQuery && IsMultipleLines(node.Span))
                {
                    var querySource = _syntaxTree.TextBuffer.GetText(node.Span);
                    AddOutliningRegionTag(node.Span, new OutliningRegionTag(false, false, "SELECT", querySource));
                }

                var nodes = node.GetChildren()
                                .SkipWhile(n => !n.FullSpan.IntersectsWith(_span))
                                .TakeWhile(n => n.FullSpan.IntersectsWith(_span));

                foreach (var syntaxNodeOrToken in nodes)
                    Visit(syntaxNodeOrToken);
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
                {
                    if (IsMultipleLines(trivia.Span))
                        AddOutliningRegionTag(trivia.Span, new OutliningRegionTag(false, false, "/**/", trivia.Text));
                }
            }
        }
    }
}