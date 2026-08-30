using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class CoalesceExpressionQuickInfoProvider : QuickInfoProvider<CoalesceExpressionSyntax>
{
    protected override QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, CoalesceExpressionSyntax node)
    {
        var keywordSpan = node.CoalesceKeyword.Span;
        return !keywordSpan.ContainsOrTouches(position)
                   ? null
                   : new QuickInfoResult(semanticModel, keywordSpan, Glyph.Function, SymbolMarkup.ForCoalesceSymbol());
    }
}
