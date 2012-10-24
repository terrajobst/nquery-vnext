using System;
using System.Linq;

using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

namespace NQuery.Language.ActiproWpf.SignatureHelp
{
    [ExportLanguageService]
    internal sealed class NQuerySignatureHelpController : IEditorDocumentTextChangeEventSink, IEditorViewSelectionChangeEventSink
    {
        public void NotifyDocumentTextChanged(SyntaxEditor editor, EditorSnapshotChangedEventArgs e)
        {
            var shouldStartSession = e.TypedText != null && e.TypedText.Any(IsTriggerChar);
            var shouldUpdateSession = HasSignatureHelpSession(editor);

            if (shouldStartSession || shouldUpdateSession)
                StartOrUpdate(editor.ActiveView);
        }

        private static bool HasSignatureHelpSession(SyntaxEditor editor)
        {
            return editor.IntelliPrompt.Sessions.OfType<ParameterInfoSession>().Any();
        }

        public void NotifyDocumentTextChanging(SyntaxEditor editor, EditorSnapshotChangingEventArgs e)
        {
        }

        public void NotifySelectionChanged(IEditorView view, EditorViewSelectionEventArgs e)
        {
            var shouldUpdateSession = HasSignatureHelpSession(view.SyntaxEditor);
            if (shouldUpdateSession)
                StartOrUpdate(view);
        }

        private static void StartOrUpdate(IEditorView view)
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