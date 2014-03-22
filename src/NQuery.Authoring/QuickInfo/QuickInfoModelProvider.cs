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
            return (from token in syntaxTree.Root.FindStartTokens(position)
                    let node = token.Parent.AncestorsAndSelf().OfType<T>().FirstOrDefault()
                    where node != null
                    select CreateModel(semanticModel, position, node)).FirstOrDefault();
        }


        protected abstract QuickInfoModel CreateModel(SemanticModel semanticModel, int position, T node);
    }
}