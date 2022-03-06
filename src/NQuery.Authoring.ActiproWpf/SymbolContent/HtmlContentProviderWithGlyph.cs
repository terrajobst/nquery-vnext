using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using ActiproSoftware.Windows.Controls.SyntaxEditor.Highlighting;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

using NQuery.Authoring.ActiproWpf.Classification;
using NQuery.Authoring.Wpf;
using NQuery.Symbols;

namespace NQuery.Authoring.ActiproWpf.SymbolContent
{
    internal sealed class HtmlContentProviderWithGlyph : HtmlContentProvider
    {
        private HtmlContentProviderWithGlyph(string htmlSnippet)
            : base(htmlSnippet)
        {
        }

        protected override Image GetImage(string source)
        {
            if (!Enum.TryParse(source, out Glyph glyph))
                return null;

            var imageSource = NQueryGlyphImageSource.Get(glyph);
            return new Image
            {
                Margin = new Thickness(0, 0, 4, 0),
                Source = imageSource,
                Stretch = Stretch.None,
                UseLayoutRounding = true
            };
        }

        public static HtmlContentProvider Create(Glyph glyph, SymbolMarkup symbolMarkup, INQueryClassificationTypes classificationTypes, IHighlightingStyleRegistry highlightingStyleRegistry)
        {
            var htmlSnippet = HtmlMarkupEmitter.GetHtml(glyph, symbolMarkup, classificationTypes, highlightingStyleRegistry);
            return new HtmlContentProviderWithGlyph(htmlSnippet);
        }
    }
}