using System;

using NQuery.Symbols;
using NQuery.Syntax;

namespace NQuery.Authoring.QuickInfo.Providers
{
    internal sealed class OrdinalOrderByColumnQuickInfoModelProvider : QuickInfoModelProvider<OrderByColumnSyntax>
    {
        protected override QuickInfoModel CreateModel(SemanticModel semanticModel, int position, OrderByColumnSyntax node)
        {
            var selectorSpan = node.ColumnSelector.Span;
            if (!selectorSpan.ContainsOrTouches(position))
                return null;

            var isOrdinal = node.ColumnSelector is LiteralExpressionSyntax;
            if (!isOrdinal)
                return null;

            var symbol = semanticModel.GetSymbol(node);
            return symbol == null ? null : new QuickInfoModel(semanticModel, selectorSpan, Glyph.Function, SymbolMarkup.ForSymbol(symbol));
        }
    }
}