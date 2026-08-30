using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class DerivedTableReferenceQuickInfoProvider : QuickInfoProvider<DerivedTableReferenceSyntax>
{
    protected override QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, DerivedTableReferenceSyntax node)
    {
        if (!node.IdentifierToken.Span.ContainsOrTouches(position))
            return null;

        var symbol = semanticModel.GetDeclaredSymbol(node);
        return symbol is null
                   ? null
                   : QuickInfoResult.ForSymbol(semanticModel, node.IdentifierToken.Span, symbol);
    }
}
