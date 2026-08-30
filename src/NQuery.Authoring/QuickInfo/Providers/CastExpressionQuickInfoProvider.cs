using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;
using NQuery.CodeAnalysis.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers;

internal sealed class CastExpressionQuickInfoProvider : QuickInfoProvider<CastExpressionSyntax>
{
    protected override QuickInfoResult? CreateResult(SemanticModel semanticModel, int position, CastExpressionSyntax node)
    {
        var keywordSpan = node.CastKeyword.Span;
        return !keywordSpan.ContainsOrTouches(position)
                   ? null
                   : new QuickInfoResult(semanticModel, keywordSpan, Glyph.Function, SymbolMarkup.ForCastSymbol());
    }
}
