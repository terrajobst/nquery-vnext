using System;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo
{
    internal sealed class CastExpressionQuickInfoModelProvider : QuickInfoModelProvider<CastExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, CastExpressionSyntax node)
        {
            var keywordSpan = node.CastKeyword.Span;
            return !keywordSpan.Contains(position)
                       ? null
                       : new QuickInfoModel(semanticModel, keywordSpan, NQueryGlyph.Function, SymbolMarkup.ForCastSymbol());
        }
    }
}