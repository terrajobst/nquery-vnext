using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class DerivedTableReferenceQuickInfoModelProvider : QuickInfoModelProvider<DerivedTableReferenceSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, DerivedTableReferenceSyntax node)
        {
            if (!node.Name.Span.ContainsOrTouches(position))
                return null;

            var symbol = semanticModel.GetDeclaredSymbol(node);
            return symbol is null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, node.Name.Span, symbol);
        }
    }
}