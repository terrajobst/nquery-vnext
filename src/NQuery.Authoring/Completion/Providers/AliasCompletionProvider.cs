using System;
using System.Collections.Generic;

using NQuery.Syntax;

namespace NQuery.Authoring.Completion.Providers
{
    internal sealed class AliasCompletionProvider : CompletionProvider<AliasSyntax>
    {
        protected override IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position, AliasSyntax node)
        {
            if (node.AsKeyword != null ||
                node.Identifier.IsMissing ||
                !node.Span.ContainsOrTouches(position))
            {
                yield break;
            }

            var identifier = node.Identifier.Text;
            yield return new CompletionItem(identifier, identifier, null, true);
        }
    }
}