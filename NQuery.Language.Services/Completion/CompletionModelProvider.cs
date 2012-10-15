using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace NQuery.Language.VSEditor.Completion
{
    [Export(typeof(ICompletionModelProvider))]
    internal sealed class CompletionModelProvider : ICompletionModelProvider
    {
        [ImportMany]
        public IEnumerable<ICompletionProvider> CompletionProviders { get; set; }

        public CompletionModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var token = GetIdentifierOrKeywordAtPosition(syntaxTree.Root, position);
            var applicableSpan = token == null ? new TextSpan(position, 0) : token.Span;

            var items = CompletionProviders.SelectMany(p => p.GetItems(semanticModel, position));
            var sortedItems = items.OrderBy(c => c.InsertionText).ToArray();

            return new CompletionModel(semanticModel, applicableSpan, sortedItems);
        }

        private static SyntaxToken GetIdentifierOrKeywordAtPosition(SyntaxNode root, int position)
        {
            var syntaxToken = root.FindTokenOnLeft(position);
            return syntaxToken.Kind.IsIdentifierOrKeyword() && syntaxToken.Span.ContainsOrTouches(position)
                       ? syntaxToken
                       : null;
        }
    }
}