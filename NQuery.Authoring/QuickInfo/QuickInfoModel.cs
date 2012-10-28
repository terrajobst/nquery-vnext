using System;

using NQuery.Symbols;

namespace NQuery.Authoring.QuickInfo
{
    public sealed class QuickInfoModel
    {
        private readonly SemanticModel _semanticModel;
        private readonly TextSpan _span;
        private readonly NQueryGlyph _glyph;
        private readonly SymbolMarkup _markup;

        public QuickInfoModel(SemanticModel semanticModel, TextSpan span, NQueryGlyph glyph, SymbolMarkup markup)
        {
            _semanticModel = semanticModel;
            _span = span;
            _glyph = glyph;
            _markup = markup;
        }

        public static QuickInfoModel ForSymbol(SemanticModel semanticModel, TextSpan span, Symbol symbol)
        {
            if (symbol.Kind == SymbolKind.BadSymbol || symbol.Kind == SymbolKind.BadTable)
                return null;

            var glyph = symbol.GetGlyph();
            var symbolMarkup = SymbolMarkup.ForSymbol(symbol);
            return new QuickInfoModel(semanticModel, span, glyph, symbolMarkup);
        }

        public SemanticModel SemanticModel
        {
            get { return _semanticModel; }
        }

        public TextSpan Span
        {
            get { return _span; }
        }

        public NQueryGlyph Glyph
        {
            get { return _glyph; }
        }

        public SymbolMarkup Markup
        {
            get { return _markup; }
        }
    }
}