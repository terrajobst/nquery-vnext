using System;
using System.Linq;

namespace NQuery.Authoring.QuickInfo
{
    public abstract class QuickInfoModelProvider<T> : IQuickInfoModelProvider
        where T: SyntaxNode
    {
        public QuickInfoModel GetModel(SemanticModel semanticModel, int position)
        {
            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            return syntaxTree.Root.FindNodes<T>(position)
                                  .Select(node => CreateModel(semanticModel, position, node))
                                  .FirstOrDefault();
        }

        protected abstract QuickInfoModel CreateModel(SemanticModel semanticModel, int position, T node);
    }
}