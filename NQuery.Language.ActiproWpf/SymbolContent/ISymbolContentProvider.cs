using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;
using NQuery.Language.Symbols;
using NQuery.Language.VSEditor;

namespace NQueryViewerActiproWpf
{
    public interface ISymbolContentProvider
    {
        IContentProvider GetContentProvider(NQueryGlyph glyph, SymbolMarkup symbolMarkup);
        IContentProvider GetContentProvider(Symbol symbol);
        IImageSourceProvider GetImageSourceProvider(NQueryGlyph glyph);
    }
}