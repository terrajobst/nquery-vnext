using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ActiproSoftware.Text;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;
using NQuery.Language.Symbols;
using NQuery.Language.VSEditor;

namespace NQueryViewerActiproWpf
{
    internal sealed class HtmlContentProviderWithGlyph : HtmlContentProvider
    {
        private readonly INQueryGlyphService _glyphService;

        private HtmlContentProviderWithGlyph(string htmlSnippet, INQueryGlyphService glyphService)
            : base(htmlSnippet)
        {
            _glyphService = glyphService;
        }

        protected override Image GetImage(string source)
        {
            NQueryGlyph glyph;
            if (!Enum.TryParse(source, out glyph))
                return null;

            var imageSource = _glyphService.GetGlyph(glyph);
            return new Image
                       {
                           Margin = new Thickness(0, 0, 4, 0),
                           Source = imageSource,
                           Stretch = Stretch.None,
                           UseLayoutRounding = true
                       };
        }

        public static HtmlContentProvider Create(NQueryGlyph glyph, SymbolMarkup symbolMarkup, INQueryGlyphService glyphService, INQueryClassificationTypes classificationTypes, IHighlightingStyleRegistry highlightingStyleRegistry)
        {
            var htmlSnippet = HtmlMarkupEmitter.GetHtml(glyph, symbolMarkup, classificationTypes, highlightingStyleRegistry);
            return new HtmlContentProviderWithGlyph(htmlSnippet, glyphService);
        }
    }
}