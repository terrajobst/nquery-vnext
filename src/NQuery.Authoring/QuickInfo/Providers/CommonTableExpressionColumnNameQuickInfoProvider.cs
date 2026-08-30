using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class CommonTableExpressionColumnNameQuickInfoProvider : QuickInfoProvider<CommonTableExpressionColumnNameSyntax>
{
    protected override QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, CommonTableExpressionColumnNameSyntax node)
    {
        var symbol = semanticModel.GetDeclaredSymbol(node);
        return symbol is null
            ? null
            : QuickInfoResult.ForSymbol(semanticModel, node.Span, symbol);
    }
}
