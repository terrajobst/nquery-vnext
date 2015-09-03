using System;

using NQuery.Symbols;
using NQuery.Text;

namespace NQuery.Authoring.QuickInfo
{
    public sealed class QuickInfoModel
    {
        private readonly SemanticModel _semanticModel;
        private readonly TextSpan _span;
        private readonly Glyph _glyph;
        private readonly SymbolMarkup _markup;

        public QuickInfoModel(SemanticModel semanticModel, TextSpan span, Glyph glyph, SymbolMarkup markup)
        {
            _semanticModel = semanticModel;
            _span = span;
            _glyph = glyph;
            _markup = markup;
        }

        public static QuickInfoModel ForSymbol(SemanticModel semanticModel, TextSpan span, Symbol symbol)
        {
            if (symbol.Kind == SymbolKind.ErrorTable)
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

        public Glyph Glyph
        {
            get { return _glyph; }
        }

        public SymbolMarkup Markup
        {
            get { return _markup; }
        }
    }
}