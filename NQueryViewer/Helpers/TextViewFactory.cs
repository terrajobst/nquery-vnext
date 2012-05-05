using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace NQueryViewer.Helpers
{
    [Export]
    internal sealed class TextViewFactory : IPartImportsSatisfiedNotification
    {
        [Import]
        public IClassificationFormatMapService ClassificationFormatMapService { get; set; }

        [Import]
        public IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [Import]
        public ITextBufferFactoryService TextBufferFactoryService { get; set; }

        [Import]
        public ITextEditorFactoryService TextEditorFactoryService { get; set; }

        [Import]
        public IEditorFormatMapService EditorFormatMapService { get; set; }

        public void OnImportsSatisfied()
        {
            // Set default editor settings

            var classificationFormatMap = ClassificationFormatMapService.GetClassificationFormatMap("text");
            classificationFormatMap.DefaultTextProperties = classificationFormatMap.DefaultTextProperties
                .SetFontRenderingEmSize(13.3333333333333333)
                .SetTypeface(new Typeface("Consolas"));
        }

        public IWpfTextViewHost CreateTextViewHost()
        {
            // Create buffer

            var contentType = ContentTypeRegistryService.GetContentType("NQuery");
            var textBuffer = TextBufferFactoryService.CreateTextBuffer(contentType);

            var textViewRoleSet = TextEditorFactoryService.CreateTextViewRoleSet(TextEditorFactoryService.DefaultRoles.Concat(new[] { "Reviewable" }));
            var textView = TextEditorFactoryService.CreateTextView(textBuffer, textViewRoleSet);
            var textViewHost = TextEditorFactoryService.CreateTextViewHost(textView, false);

            // Set the appearance of collapsed text
            textViewHost.TextView.VisualElement.Resources["CollapsedTextForeground"] = new SolidColorBrush(Color.FromRgb(0xA5, 0xA5, 0xA5));
            textViewHost.HostControl.Resources["outlining.verticalrule.foreground"] = new SolidColorBrush(Color.FromRgb(0xA5, 0xA5, 0xA5));

            // Set the background color of glyph margin
            const string indicatorMargin = "Indicator Margin";
            var backgroundColor = Color.FromRgb(0xF0, 0xF0, 0xF0);
            var editorFormatMap = EditorFormatMapService.GetEditorFormatMap(textViewHost.TextView);
            var resourceDictionary = editorFormatMap.GetProperties(indicatorMargin);
            resourceDictionary["BackgroundColor"] = backgroundColor;
            resourceDictionary["Background"] = new SolidColorBrush(backgroundColor);
            editorFormatMap.SetProperties(indicatorMargin, resourceDictionary);

            return textViewHost;
        }
    }
}