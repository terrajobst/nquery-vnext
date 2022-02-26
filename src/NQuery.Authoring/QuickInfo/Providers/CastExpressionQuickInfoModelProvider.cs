using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class CastExpressionQuickInfoModelProvider : QuickInfoModelProvider<CastExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, CastExpressionSyntax node)
        {
            var keywordSpan = node.CastKeyword.Span;
            return !keywordSpan.ContainsOrTouches(position)
                       ? null
                       : new QuickInfoModel(semanticModel, keywordSpan, Glyph.Function, SymbolMarkup.ForCastSymbol());
        }
    }
}