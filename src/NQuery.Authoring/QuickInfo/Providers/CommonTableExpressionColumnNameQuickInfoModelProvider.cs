using System;

using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class CommonTableExpressionColumnNameQuickInfoModelProvider : QuickInfoModelProvider<CommonTableExpressionColumnNameSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, CommonTableExpressionColumnNameSyntax node)
        {
            var symbol = semanticModel.GetDeclaredSymbol(node);
            return symbol == null
                ? null
                : QuickInfoModel.ForSymbol(semanticModel, node.Span, symbol);
        }
    }
}