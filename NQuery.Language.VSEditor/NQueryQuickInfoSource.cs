using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace NQuery.Language.VSEditor
{
    internal sealed class NQueryQuickInfoSource : IQuickInfoSource
    {
        private readonly INQuerySyntaxTreeManager _syntaxTreeManager;

        public NQueryQuickInfoSource(INQuerySyntaxTreeManager syntaxTreeManager)
        {
            _syntaxTreeManager = syntaxTreeManager;
        }

        public void Dispose()
        {
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            var syntaxTree = _syntaxTreeManager.SyntaxTree;
            if (syntaxTree == null)
                return;

            var currentSnapshot = session.TextView.TextBuffer.CurrentSnapshot;
            var triggerPoint = session.GetTriggerPoint(currentSnapshot);
            if (triggerPoint == null)
                return;

            var position = triggerPoint.Value.Position;
            var node = FindNodeWithToken(syntaxTree.Root, position);
            if (node == null)
                return;

            var span = new Span(node.Span.Start, node.Span.Length);
            applicableToSpan = currentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeNegative);
            quickInfoContent.Add(node.GetType().ToString());
        }

        private static SyntaxNode FindNodeWithToken(SyntaxNode root, int position)
        {
            if (!root.FullSpan.Contains(position))
                return null;

            var nodes = root.GetChildren()
                .SkipWhile(n => !n.Span.Contains(position))
                .TakeWhile(n => n.Span.Contains(position));

            foreach (var syntaxNodeOrToken in nodes)
            {
                if (syntaxNodeOrToken.IsToken)
                    return root;

                var result = FindNodeWithToken(syntaxNodeOrToken.AsNode(), position);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}