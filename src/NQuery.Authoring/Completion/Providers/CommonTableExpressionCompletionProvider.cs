using System;
using System.Collections.Generic;

using NQuery.Syntax;

namespace NQuery.Authoring.Completion.Providers
{
    internal sealed class CommonTableExpressionCompletionProvider : CompletionProvider<CommonTableExpressionSyntax>
    {
        protected override IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position, CommonTableExpressionSyntax node)
        {
            if (node.RecursiveKeyword != null ||
                node.Name.IsMissing ||
                !node.Name.FullSpan.ContainsOrTouches(position))
            {
                yield break;
            }

            var identifier = node.Name.Text;
            yield return new CompletionItem(identifier, identifier, null, true);
        }
    }
}