using System;
using System.ComponentModel.Composition;
using System.Windows.Media;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

using NQuery.Authoring.VSEditorWpf;
using NQuery.Authoring.VSEditorWpf.Selection;

using NQueryViewer.Editor;

namespace NQueryViewer.VSEditor
{
    [Export(typeof(IEditorViewFactory))]
    [Export(typeof(IVSEditorViewFactory))]
    internal sealed class VSEditorViewFactory : IVSEditorViewFactory, IPartImportsSatisfiedNotification
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

        [Import]
        public INQuerySelectionProviderService SelectionProviderService { get; set; }

        public void OnImportsSatisfied()
        {
            // Set default editor settings

            var classificationFormatMap = ClassificationFormatMapService.GetClassificationFormatMap("text");
            classificationFormatMap.DefaultTextProperties = classificationFormatMap.DefaultTextProperties
                .SetFontRenderingEmSize(13.3333333333333333)
                .SetTypeface(new Typeface("Consolas"));
        }

        public IVSEditorView CreateEditorView()
        {
            var textViewHost = CreateTextViewHost();
            var textView = textViewHost.TextView;
            var workspace = textView.TextBuffer.GetWorkspace();
            var selectionProvider = SelectionProviderService.GetSelectionProvider(textView);
            return new VSEditorView(workspace, textViewHost, selectionProvider);
        }

        IEditorView IEditorViewFactory.CreateEditorView()
        {
            return CreateEditorView();
        }

        private IWpfTextViewHost CreateTextViewHost()
        {
            // Create buffer

            var contentType = ContentTypeRegistryService.GetContentType("NQuery");
            var textBuffer = TextBufferFactoryService.CreateTextBuffer(contentType);

//            textBuffer.Insert(0, "hello");
//
//            var previous = textBuffer.CurrentSnapshot;
//
//            using (var textEdit = textBuffer.CreateEdit())
//            {
//                // hlxo
//                // textEdit.Delete(1, 1);
//                // textEdit.Replace(3, 1, "x");
//
//                // hlxo
//                // textEdit.Replace(3, 1, "x");
//                // textEdit.Delete(1, 1);
//
//                // helxo
//                // textEdit.Replace(3, 1, "x");
//
//                // he/**/
//                // textEdit.Replace(2, 2, "/*");
//                // textEdit.Replace(3, 2, "*/");
//
//                textEdit.Apply();
//            }
//
//            var text = textBuffer.CurrentSnapshot.GetText();
//            var changes = previous.Version.Changes;

            var textView = TextEditorFactoryService.CreateTextView(textBuffer);
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

        public int Priority
        {
            get { return 1; }
        }

        public string DisplayName
        {
            get { return "Visual Studio Editor"; }
        }
    }
}