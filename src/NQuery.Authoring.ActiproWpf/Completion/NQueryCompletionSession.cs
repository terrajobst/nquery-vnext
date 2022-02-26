using System.Windows.Input;

using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;

namespace NQuery.Authoring.ActiproWpf.Completion
{
    internal sealed class NQueryCompletionSession : CompletionSession
    {
        protected override void OnViewKeyDown(IEditorView view, KeyEventArgs e)
        {
            // The base is eating the tab key - we want to forward it to the view.
            var isTab = e.Key == Key.Tab && e.KeyboardDevice.Modifiers == ModifierKeys.None;
            if (!isTab)
            {
                base.OnViewKeyDown(view, e);
            }
            else
            {
                Commit();
                e.Handled = false;
            }
        }
    }
}