using System;
using System.Windows.Media;

using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

using NQuery.Authoring.Wpf;

namespace NQuery.Authoring.ActiproWpf.SymbolContent
{
    internal sealed class GlyphImageProvider : IImageSourceProvider
    {
        private readonly NQueryGlyph _glyph;

        public GlyphImageProvider(NQueryGlyph glyph)
        {
            _glyph = glyph;
        }

        public ImageSource GetImageSource()
        {
            return NQueryGlyphImageSource.Get(_glyph);
        }
    }
}