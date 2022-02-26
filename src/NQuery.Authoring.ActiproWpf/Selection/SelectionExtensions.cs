using System.Windows.Input;

using ActiproSoftware.Windows.Controls.SyntaxEditor;

namespace NQuery.Authoring.ActiproWpf.Selection
{
    public static class SelectionExtensions
    {
        public static void RegisterSelectionCommands(this SyntaxEditor syntaxEditor)
        {
            syntaxEditor.CommandBindings.Add(new ExtendSelectionAction().CreateCommandBinding(ExtendSelectionAction.Command));
            syntaxEditor.CommandBindings.Add(new ShrinkSelectionAction().CreateCommandBinding(ShrinkSelectionAction.Command));

            syntaxEditor.InputBindings.Add(new InputBinding(ExtendSelectionAction.Command, new KeyGesture(Key.W, ModifierKeys.Control)));
            syntaxEditor.InputBindings.Add(new InputBinding(ShrinkSelectionAction.Command, new KeyGesture(Key.W, ModifierKeys.Control | ModifierKeys.Shift)));
        }
    }
}