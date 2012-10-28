using System;
using System.Windows.Media;

namespace NQuery.Authoring.Wpf
{
    public interface INQueryGlyphService
    {
        ImageSource GetGlyph(NQueryGlyph glyph);
    }
}