using System.Windows.Input;

using ActiproSoftware.Windows.Controls.SyntaxEditor;

namespace NQuery.Authoring.ActiproWpf.Commenting;

public static class CommentingExtensions
{
    extension(SyntaxEditor syntaxEditor)
    {
        public void RegisterCommentingCommands()
        {
            syntaxEditor.CommandBindings.Add(new ToggleSingleLineCommentAction().CreateCommandBinding(ToggleSingleLineCommentAction.Command));
            syntaxEditor.CommandBindings.Add(new ToggleMultiLineCommentAction().CreateCommandBinding(ToggleMultiLineCommentAction.Command));

            syntaxEditor.InputBindings.Add(new InputBinding(ToggleSingleLineCommentAction.Command, new KeyGesture(Key.Oem2, ModifierKeys.Control | ModifierKeys.Alt)));
            syntaxEditor.InputBindings.Add(new InputBinding(ToggleMultiLineCommentAction.Command, new KeyGesture(Key.Oem2, ModifierKeys.Control | ModifierKeys.Shift)));
        }
    }
}
