using System;
using System.Windows.Input;

using ActiproSoftware.Windows.Controls.SyntaxEditor;

namespace NQuery.Authoring.ActiproWpf.CodeActions
{
    public static class CodeActionExtensions
    {
        public static void RegisterCodeActionCommands(this SyntaxEditor syntaxEditor)
        {
            syntaxEditor.CommandBindings.Add(new ExpandCodeActionListEditAction().CreateCommandBinding(ExpandCodeActionListEditAction.Command));
            syntaxEditor.InputBindings.Add(new InputBinding(ExpandCodeActionListEditAction.Command, new KeyGesture(Key.OemPeriod, ModifierKeys.Control)));
        }
    }
}