using System;
using System.ComponentModel.Composition;

using NQuery.Language.Symbols;

namespace NQuery.Language.Services.QuickInfo
{
    [Export(typeof(IQuickInfoModelProvider))]
    internal sealed class CoalesceExpressionQuickInfoModelProvider : QuickInfoModelProvider<CoalesceExpressionSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, CoalesceExpressionSyntax node)
        {
            var keywordSpan = node.CoalesceKeyword.Span;
            return !keywordSpan.Contains(position)
                       ? null
                       : new QuickInfoModel(semanticModel, keywordSpan, NQueryGlyph.Function, SymbolMarkup.ForCoalesceSymbol());
        }
    }
}