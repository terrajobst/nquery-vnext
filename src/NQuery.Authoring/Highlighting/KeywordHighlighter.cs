using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Text;

namespace NQuery.Authoring.Highlighting
{
    public abstract class KeywordHighlighter<T> : IHighlighter
        where T: SyntaxNode
    {
        public IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindToken(position);

            for (var current = token.Parent; current != null; current = current.Parent)
            {
                var node = current as T;
                if (node != null)
                {
                    var textSpans = GetHighlights(semanticModel, node, position).ToImmutableArray();
                    if (textSpans.Any(s => s.ContainsOrTouches(position)))
                        return textSpans;
                }
            }

            return Enumerable.Empty<TextSpan>();
        }

        protected abstract IEnumerable<TextSpan> GetHighlights(SemanticModel semanticModel, T node, int position);
    }
}