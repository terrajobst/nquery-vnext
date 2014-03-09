using System;

using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

using NQuery.Symbols;

namespace NQuery.Authoring.ActiproWpf.SymbolContent
{
    public interface INQuerySymbolContentProvider
    {
        IContentProvider GetContentProvider(NQueryGlyph glyph, SymbolMarkup symbolMarkup);
        IContentProvider GetContentProvider(Symbol symbol);
        IImageSourceProvider GetImageSourceProvider(NQueryGlyph glyph);
    }
}