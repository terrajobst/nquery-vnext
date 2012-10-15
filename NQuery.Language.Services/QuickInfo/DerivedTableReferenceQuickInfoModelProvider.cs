using System.ComponentModel.Composition;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class DerivedTableReferenceQuickInfoModelProvider : QuickInfoModelProvider<DerivedTableReferenceSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, DerivedTableReferenceSyntax node)
        {
            if (!node.Name.Span.Contains(position))
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(node);
            return symbol == null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, node.Name.Span, symbol);
        }
    }
}