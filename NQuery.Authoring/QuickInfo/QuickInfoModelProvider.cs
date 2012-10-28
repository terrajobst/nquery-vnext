using System;
using System.Linq;

namespace NQuery.Language.Services.QuickInfo
{
    internal abstract class QuickInfoModelProvider<T> : IQuickInfoModelProvider
        where T: SyntaxNode
    {
        public QuickInfoModel GetModel(SemanticModel semanticModel, int position)
        {
            var token = semanticModel.Compilation.SyntaxTree.Root.FindToken(position);
            if (token == null || !token.Span.Contains(position) || token.Parent == null)
                return null;

            var node = token.Parent.AncestorsAndSelf().OfType<T>().FirstOrDefault();
            return node == null
                       ? null
                       : CreateModel(semanticModel, position, node);
        }

        protected abstract QuickInfoModel CreateModel(SemanticModel semanticModel, int position, T node);
    }
}