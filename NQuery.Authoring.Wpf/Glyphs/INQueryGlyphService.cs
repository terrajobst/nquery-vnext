using System;
using System.Windows.Media;

using NQuery.Language.Services;

namespace NQuery.Language.Wpf
{
    public interface INQueryGlyphService
    {
        ImageSource GetGlyph(NQueryGlyph glyph);
    }
}