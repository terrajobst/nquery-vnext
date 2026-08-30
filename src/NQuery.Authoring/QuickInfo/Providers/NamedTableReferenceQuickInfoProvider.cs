using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class NamedTableReferenceQuickInfoProvider : QuickInfoProvider<NamedTableReferenceSyntax>
{
    protected override QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, NamedTableReferenceSyntax node)
    {
        var symbol = semanticModel.GetDeclaredSymbol(node);
        if (symbol is null)
            return null;

        if (node.IdentifierToken.Span.ContainsOrTouches(position))
        {
            var span = node.IdentifierToken.Span;
            return QuickInfoResult.ForSymbol(semanticModel, span, symbol.Table);
        }

        if (node.Alias is not null && node.Alias.IdentifierToken.Span.ContainsOrTouches(position))
        {
            var span = node.Alias.IdentifierToken.Span;
            return QuickInfoResult.ForSymbol(semanticModel, span, symbol);
        }

        return null;
    }
}
