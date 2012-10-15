using System;
using System.Collections.Generic;

using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Outlining.Implementation;

using NQuery.Language;

namespace NQueryViewerActiproWpf
{
    internal sealed class NQueryOutliningSource : RangeOutliningSourceBase
    {
        public NQueryOutliningSource(ITextSnapshot snapshot, SyntaxTree syntaxTree)
            : base(snapshot)
        {
            var result = new List<Tuple<TextRange, IOutliningNodeDefinition>>();
            var worker = new Worker(snapshot, syntaxTree, result);
            worker.Visit(syntaxTree.Root);

            foreach (var tuple in result)
                AddNode(tuple.Item1, tuple.Item2);
        }

        private sealed class Worker
        {
            private readonly ITextSnapshot _snapshot;
            private readonly SyntaxTree _syntaxTree;
            private readonly List<Tuple<TextRange, IOutliningNodeDefinition>> _result;

            public Worker(ITextSnapshot snapshot, SyntaxTree syntaxTree, List<Tuple<TextRange, IOutliningNodeDefinition>> result)
            {
                _snapshot = snapshot;
                _syntaxTree = syntaxTree;
                _result = result;
            }

            private void AddOutlineRegion(TextSpan textSpan, string text, string hint)
            {
                IOutliningNodeDefinition nodeDefinition = new OutliningNodeDefinition("NQueryNode")
                {
                    DefaultCollapsedContent = text,                    
                    IsImplementation = false
                };

                var textBuffer = _syntaxTree.TextBuffer;
                var snapshotRange = textBuffer.ToSnapshotRange(_snapshot, textSpan);
                var textRange = snapshotRange.TextRange;

                _result.Add(Tuple.Create(textRange, nodeDefinition));
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