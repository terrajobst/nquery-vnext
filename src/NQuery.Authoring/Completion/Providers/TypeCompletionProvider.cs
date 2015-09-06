using System;
using System.Collections.Generic;
using System.Linq;

using NQuery.Syntax;

namespace NQuery.Authoring.Completion.Providers
{
    internal sealed class TypeCompletionProvider : CompletionProvider<CastExpressionSyntax>
    {
        protected override IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position, CastExpressionSyntax node)
        {
            if (node.AsKeyword.IsMissing || position < node.AsKeyword.Span.End)
                return Enumerable.Empty<CompletionItem>();

            return from typeName in SyntaxFacts.GetTypeNames()
                   select GetCompletionItem(typeName);
        }

        private static CompletionItem GetCompletionItem(string typeName)
        {
            return new CompletionItem(typeName, typeName, typeName, Glyph.Type);
        }
    }
}