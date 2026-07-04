using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class ExpressionSelectColumnQuickInfoModelProvider : QuickInfoModelProvider<ExpressionSelectColumnSyntax>
{
    protected override QuickInfoModel? CreateModel(SemanticModel semanticModel, int position, ExpressionSelectColumnSyntax node)
    {
        if (node.Alias is null || !node.Alias.IdentifierToken.Span.ContainsOrTouches(position))
            return null;

        var symbol = semanticModel.GetDeclaredSymbol(node);
        return symbol is null
                   ? null
                   : QuickInfoModel.ForSymbol(semanticModel, node.Alias.IdentifierToken.Span, symbol);
    }
}
