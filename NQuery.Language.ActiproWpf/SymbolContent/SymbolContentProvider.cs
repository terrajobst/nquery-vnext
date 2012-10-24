using System;
using System.ComponentModel.Composition;

using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

using NQuery.Language.ActiproWpf.Classification;
using NQuery.Language.Services;
using NQuery.Language.Symbols;

namespace NQuery.Language.ActiproWpf.SymbolContent
{
    [Export(typeof(ISymbolContentProvider))]
    internal sealed class SymbolContentProvider : ISymbolContentProvider
    {
        [Import]
        public INQueryClassificationTypes ClassificationTypes { get; set; }

        [Import]
        public INQueryGlyphService GlyphService { get; set; }

        public IContentProvider GetContentProvider(NQueryGlyph glyph, SymbolMarkup symbolMarkup)
        {
            var classificationTypes = ClassificationTypes;
            var registry = AmbientHighlightingStyleRegistry.Instance;
            var glyphService = GlyphService;
            var contentProvider = HtmlContentProviderWithGlyph.Create(glyph, symbolMarkup, glyphService, classificationTypes, registry);
            return contentProvider;
        }

        public IContentProvider GetContentProvider(Symbol symbol)
        {
            return GetContentProvider(symbol.GetGlyph(), SymbolMarkup.ForSymbol(symbol));
        }

        public IImageSourceProvider GetImageSourceProvider(NQueryGlyph glyph)
        {
            return new GlyphImageProvider(GlyphService, glyph);
        }
    }
}