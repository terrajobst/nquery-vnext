using System;

using NQuery.Symbols;
using NQuery.Text;

namespace NQuery.Authoring.QuickInfo
{
    public sealed class QuickInfoModel
    {
        public QuickInfoModel(SemanticModel semanticModel, TextSpan span, Glyph glyph, SymbolMarkup markup)
        {
            SemanticModel = semanticModel;
            Span = span;
            Glyph = glyph;
            Markup = markup;
        }

        public static QuickInfoModel ForSymbol(SemanticModel semanticModel, TextSpan span, Symbol symbol)
        {
            if (symbol.Kind == SymbolKind.ErrorTable)
                return null;

            var glyph = symbol.GetGlyph();
            var symbolMarkup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, glyph, symbolMarkup);
        }

        public SemanticModel SemanticModel { get; }

        public TextSpan Span { get; }

        public Glyph Glyph { get; }

        public SymbolMarkup Markup { get; }
    }
}