using System;
using System.Windows.Media;

namespace NQuery.Language.Services
{
    public interface INQueryGlyphService
    {
        ImageSource GetGlyph(NQueryGlyph glyph);
    }
}