using System;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class CoalesceExpressionQuickInfoModelProvider : QuickInfoModelProvider<CoalesceExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, CoalesceExpressionSyntax node)
        {
            var keywordSpan = node.CoalesceKeyword.Span;
            return !keywordSpan.ContainsOrTouches(position)
                       ? null
                       : new QuickInfoModel(semanticModel, keywordSpan, Glyph.Function, SymbolMarkup.ForCoalesceSymbol());
        }
    }
}