using System.Windows.Input;

using ActiproSoftware.Windows.Controls.SyntaxEditor;

namespace NQuery.Authoring.ActiproWpf.CodeActions;

public static class CodeActionExtensions
{
    extension(SyntaxEditor syntaxEditor)
    {
        public void RegisterCodeActionCommands()
        {
            ThrowIfNull(syntaxEditor);

            syntaxEditor.CommandBindings.Add(new ExpandCodeActionListEditAction().CreateCommandBinding(ExpandCodeActionListEditAction.Command));
            syntaxEditor.InputBindings.Add(new InputBinding(ExpandCodeActionListEditAction.Command, new KeyGesture(Key.OemPeriod, ModifierKeys.Control)));
        }
    }
}
