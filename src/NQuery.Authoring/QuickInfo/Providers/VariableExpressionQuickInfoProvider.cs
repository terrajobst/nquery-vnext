using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class VariableExpressionQuickInfoProvider : QuickInfoProvider<VariableExpressionSyntax>
{
    protected override QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, VariableExpressionSyntax node)
    {
        var symbol = semanticModel.GetSymbol(node);
        return symbol is null
                   ? null
                   : QuickInfoResult.ForSymbol(semanticModel, node.Span, symbol);
    }
}
