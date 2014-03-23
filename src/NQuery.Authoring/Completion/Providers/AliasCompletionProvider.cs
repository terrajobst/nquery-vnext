using System;
using System.Collections.Generic;

using NQuery.Syntax;

namespace NQuery.Authoring.Completion.Providers
{
    // TODO: Add test coverage

    internal sealed class AliasCompletionProvider : ICompletionProvider
    {
        public IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = syntaxTree.Root.FindTokenOnLeft(position);
            var node = token.Parent as AliasSyntax;
            if (node == null ||
                node.AsKeyword != null ||
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