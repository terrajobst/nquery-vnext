using System.Linq;
using System.Windows.Input;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

namespace NQueryViewerActiproWpf
{
    [ExportLanguageService(typeof(IEditorViewTextInputEventSink))]
    internal sealed class NQueryCompletionController : IEditorViewTextInputEventSink
    {
        public void NotifyTextInput(IEditorView view, TextCompositionEventArgs e)
        {
            if (e.Text.Any(IsTriggerChar))
            {
                view.IntelliPrompt.RequestCompletionSession();
            }
            else if (e.Text.Any(IsCommitChar))
            {
                var session = view.SyntaxEditor.IntelliPrompt.Sessions.OfType<CompletionSession>().FirstOrDefault();
                if (session != null)
                    session.Commit();
            }
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