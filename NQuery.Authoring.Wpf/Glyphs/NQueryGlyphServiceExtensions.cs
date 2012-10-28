using System;
using System.Windows.Media;

using NQuery.Symbols;

namespace NQuery.Authoring.Wpf
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