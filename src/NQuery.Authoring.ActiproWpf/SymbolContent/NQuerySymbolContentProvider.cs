using System;

using ActiproSoftware.Text.Utility;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

using NQuery.Authoring.ActiproWpf.Classification;
using NQuery.Symbols;

namespace NQuery.Authoring.ActiproWpf.SymbolContent
{
    internal sealed class NQuerySymbolContentProvider : INQuerySymbolContentProvider
    {
        private readonly IServiceLocator _serviceLocator;

        public NQuerySymbolContentProvider(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        private INQueryClassificationTypes ClassificationTypes
        {
            get { return _serviceLocator.GetService<INQueryClassificationTypes>(); }
        }

        public IContentProvider GetContentProvider(Glyph glyph, SymbolMarkup symbolMarkup)
        {
            var classificationTypes = ClassificationTypes;
            var registry = AmbientHighlightingStyleRegistry.Instance;
            return HtmlContentProviderWithGlyph.Create(glyph, symbolMarkup, classificationTypes, registry);
        }

        public IContentProvider GetContentProvider(Symbol symbol)
        {
            return GetContentProvider(symbol.GetGlyph(), SymbolMarkup.ForSymbol(symbol));
        }

        public IImageSourceProvider GetImageSourceProvider(Glyph glyph)
        {
            return new GlyphImageProvider(glyph);
        }
    }
}