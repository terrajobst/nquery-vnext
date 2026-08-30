using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class WildcardSelectColumnQuickInfoProvider : QuickInfoProvider<WildcardSelectColumnSyntax>
{
    protected override QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, WildcardSelectColumnSyntax node)
    {
        var tableName = node.IdentifierToken;
        if (tableName is null || !tableName.Span.ContainsOrTouches(position))
            return null;

        var tableInstanceSymbol = semanticModel.GetTableInstance(node);
        return tableInstanceSymbol is null
                   ? null
                   : QuickInfoResult.ForSymbol(semanticModel, tableName.Span, tableInstanceSymbol);
    }
}
