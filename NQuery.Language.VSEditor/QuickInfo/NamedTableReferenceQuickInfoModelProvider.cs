using System.ComponentModel.Composition;
using System.Linq;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class NamedTableReferenceQuickInfoModelProvider : IQuickInfoModelProvider
    {
        public QuickInfoModel GetModel(SemanticModel semanticModel, int position)
        {
            var token = semanticModel.Compilation.SyntaxTree.Root.FindToken(position);
            if (token == null || !token.Span.Contains(position) || token.Parent == null)
                return null;

            var node = token.Parent.AncestorsAndSelf().OfType<NamedTableReferenceSyntax>().FirstOrDefault();
            if (node == null)
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(node);
            if (symbol == null)
                return null;

            if (node.TableName.Span.Contains(position))
                return new QuickInfoModel(node.TableName, symbol.Table);

            if (node.Alias != null && node.Alias.Span.Contains(position))
                return new QuickInfoModel(node.Alias, symbol);

            return null;
        }
    }
}