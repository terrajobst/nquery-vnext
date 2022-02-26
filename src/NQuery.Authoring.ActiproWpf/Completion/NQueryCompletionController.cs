using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

namespace NQuery.Authoring.ActiproWpf.Completion
{
    internal sealed class NQueryCompletionController : IEditorDocumentTextChangeEventSink
    {
        public void NotifyDocumentTextChanged(SyntaxEditor editor, EditorSnapshotChangedEventArgs e)
        {
            var typedText = e.TypedText;
            if (string.IsNullOrEmpty(typedText))
                return;

            if (typedText.Any(IsCommitChar))
                Commit(editor.ActiveView);

            if (typedText.Any(IsTriggerChar))
                Start(editor.ActiveView);
        }

        public void NotifyDocumentTextChanging(SyntaxEditor editor, EditorSnapshotChangingEventArgs e)
        {
        }

        private static void Start(IEditorView view)
        {
            view.IntelliPrompt.RequestCompletionSession();
        }

        private static void Commit(IEditorView view)
        {
            var session = view.SyntaxEditor.IntelliPrompt.Sessions.OfType<CompletionSession>().FirstOrDefault();
            if (session != null)
                session.Commit();
        }

        private static bool IsTriggerChar(char c)
        {
            return char.IsLetter(c) ||
                   c == '_' ||
                   c == '.';
        }

        private static bool IsCommitChar(char c)
        {
            return char.IsWhiteSpace(c) ||
                   c == '.' ||
                   c == ',' ||
                   c == '(' ||
                   c == ')';
        }
    }
}