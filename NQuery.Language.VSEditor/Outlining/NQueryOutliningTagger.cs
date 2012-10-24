using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

using NQuery.Language.VSEditor.Document;

namespace NQuery.Language.VSEditor.Outlining
{
    internal sealed class NQueryOutliningTagger : AsyncTagger<IOutliningRegionTag, RawOutliningRegionTag> 
    {
        private readonly INQueryDocument _document;

        public NQueryOutliningTagger(INQueryDocument document)
        {
            _document = document;
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
            InvalidateTags();
        }

        private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs eventArgs)
        {
            InvalidateTags();
        }

        protected override async Task<Tuple<ITextSnapshot, IEnumerable<RawOutliningRegionTag>>> GetRawTagsAsync()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var snapshot = _document.GetTextSnapshot(syntaxTree);
            var result = new List<RawOutliningRegionTag>();
            var visitor = new Visitor(syntaxTree, result);
            visitor.Visit(syntaxTree.Root);
            return Tuple.Create(snapshot, result.AsEnumerable());
        }

        protected override ITagSpan<IOutliningRegionTag> CreateTagSpan(ITextSnapshot snapshot, RawOutliningRegionTag rawTag)
        {
            var textSpan = rawTag.TextSpan;
            var span = new Span(textSpan.Start, textSpan.Length);
            var snapshotSpan = new SnapshotSpan(snapshot, span);
            var tag = new OutliningRegionTag(false, false, rawTag.Text, rawTag.Hint);
            var tagSpan = new TagSpan<IOutliningRegionTag>(snapshotSpan, tag);
            return tagSpan;
        }

        private sealed class Visitor
        {
            private readonly SyntaxTree _syntaxTree;
            private readonly List<RawOutliningRegionTag> _outlineRegions;

            public Visitor(SyntaxTree syntaxTree, List<RawOutliningRegionTag> outlineRegions)
            {
                _syntaxTree = syntaxTree;
                _outlineRegions = outlineRegions;
            }

            private void AddOutlineRegion(TextSpan textSpan, string text, string hint)
            {
                var outliningData = new RawOutliningRegionTag(textSpan, text, hint);
                _outlineRegions.Add(outliningData);
            }

            private bool IsMultipleLines(TextSpan textSpan)
            {
                var start = _syntaxTree.TextBuffer.GetLineFromPosition(textSpan.Start);
                var end = _syntaxTree.TextBuffer.GetLineFromPosition(textSpan.End);
                return start.Index != end.Index;
            }

            public void Visit(SyntaxNode node)
            {
                if (node.Kind == SyntaxKind.SelectQuery && IsMultipleLines(node.Span))
                {
                    var querySource = _syntaxTree.TextBuffer.GetText(node.Span);
                    AddOutlineRegion(node.Span, "SELECT", querySource);
                }

                foreach (var syntaxNodeOrToken in node.ChildNodesAndTokens())
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
                        AddOutlineRegion(trivia.Span, "/**/", trivia.Text);
                }
            }
        }
    }
}