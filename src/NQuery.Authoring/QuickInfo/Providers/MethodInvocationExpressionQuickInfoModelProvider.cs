using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class MethodInvocationExpressionQuickInfoModelProvider : QuickInfoModelProvider<MethodInvocationExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, MethodInvocationExpressionSyntax node)
        {
            if (!node.Name.Span.ContainsOrTouches(position))
                return null;

            var symbol = semanticModel.GetSymbol(node);
            return symbol == null
                       ? null
                       : QuickInfoModel.ForSymbol(semanticModel, node.Name.Span, symbol);
        }
    }
}