using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class NameExpressionQuickInfoModelProvider : QuickInfoModelProvider<NameExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, NameExpressionSyntax node)
        {
            var symbol = semanticModel.GetSymbol(node);
            return symbol is null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, node.Span, symbol);
        }
    }
}