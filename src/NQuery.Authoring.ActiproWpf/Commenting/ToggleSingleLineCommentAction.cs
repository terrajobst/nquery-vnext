using System;
using System.Windows.Input;

using ActiproSoftware.Windows.Controls.SyntaxEditor;

using NQuery.Authoring.Commenting;
using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Commenting
{
    public class ToggleSingleLineCommentAction : ToggleCommentAction
    {
        private static readonly Lazy<RoutedCommand> LazyCommand = new Lazy<RoutedCommand>(() => new RoutedCommand("Toggle Single Line Comment", typeof(SyntaxEditor)));

        public static RoutedCommand Command
        {
            get { return LazyCommand.Value; }
        }

        protected override SyntaxTree ToggleComment(SyntaxTree syntaxTree, TextSpan textSpan)
        {
            return syntaxTree.ToggleSingleLineComment(textSpan);
        }
    }
}