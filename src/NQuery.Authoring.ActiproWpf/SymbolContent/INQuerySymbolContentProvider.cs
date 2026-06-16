using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;

namespace NQuery.Authoring.ActiproWpf.SymbolContent;

public interface INQuerySymbolContentProvider
{
    IContentProvider GetContentProvider(Glyph glyph, SymbolMarkup symbolMarkup);
    IContentProvider GetContentProvider(Symbol symbol);
    IImageSourceProvider GetImageSourceProvider(Glyph glyph);
}
