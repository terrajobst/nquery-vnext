using System;
using System.ComponentModel.Composition;

using NQuery.Symbols;

namespace NQuery.Authoring.QuickInfo
{
    [Export(typeof(IQuickInfoModelProvider))]
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