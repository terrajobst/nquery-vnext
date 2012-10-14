using System.Linq;
using NQuery.Language.Symbols;

namespace NQuery.Language.VSEditor
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
            if (node == null)
                return null;

            if (!IsMatch(semanticModel, position, node))
                return null;

            var symbol = GetSymbol(semanticModel, position, node);
            if (symbol == null)
                return null;

            return new QuickInfoModel(token, symbol);
        }

        protected abstract bool IsMatch(SemanticModel semanticModel, int position, T node);
        protected abstract Symbol GetSymbol(SemanticModel semanticModel, int position, T node);
    }
}