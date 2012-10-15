using System.Windows.Media;

namespace NQuery.Language.VSEditor
{
    public interface INQueryGlyphService
    {
        ImageSource GetGlyph(NQueryGlyph glyph);
    }
}