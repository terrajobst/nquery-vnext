using System;

using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

using NQuery.Language.Services;
using NQuery.Language.Symbols;

namespace NQuery.Language.ActiproWpf.SymbolContent
{
    public interface ISymbolContentProvider
    {
        IContentProvider GetContentProvider(NQueryGlyph glyph, SymbolMarkup symbolMarkup);
        IContentProvider GetContentProvider(Symbol symbol);
        IImageSourceProvider GetImageSourceProvider(NQueryGlyph glyph);
    }
}