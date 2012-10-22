using System.Linq;
using ActiproSoftware.Windows.Controls.SyntaxEditor;

namespace NQueryViewerActiproWpf.SignatureHelp
{
    [ExportLanguageService]
    internal sealed class NQuerySignatureHelpController : IEditorDocumentTextChangeEventSink
    {
        public void NotifyDocumentTextChanged(SyntaxEditor editor, EditorSnapshotChangedEventArgs e)
        {
            var typedText = e.TypedText;
            if (string.IsNullOrEmpty(typedText))
                return;

            if (typedText.Any(IsTriggerChar))
                Start(editor.ActiveView);
        }

        public void NotifyDocumentTextChanging(SyntaxEditor editor, EditorSnapshotChangingEventArgs e)
        {
        }

        private static void Start(IEditorView view)
        {
            view.IntelliPrompt.RequestParameterInfoSession();
        }

        private static bool IsTriggerChar(char c)
        {
            return c == '(' ||
                   c == ',';
        }
    }
}