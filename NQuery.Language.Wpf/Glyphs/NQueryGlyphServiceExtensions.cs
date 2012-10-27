using System;
using System.Windows.Media;

using NQuery.Language.Services;
using NQuery.Language.Symbols;

namespace NQuery.Language.Wpf
{
    public static class NQueryGlyphServiceExtensions
    {
        public static ImageSource GetGlyph(this INQueryGlyphService glyphService, Symbol symbol)
        {
            var glyph = symbol.GetGlyph();
            return glyphService.GetGlyph(glyph);
        }
    }
}