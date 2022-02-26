using System.Windows.Input;

using ActiproSoftware.Windows.Controls.SyntaxEditor;

namespace NQuery.Authoring.ActiproWpf.Selection
{
    public class ExtendSelectionAction : SelectionAction
    {
        private static readonly Lazy<RoutedCommand> LazyCommand = new Lazy<RoutedCommand>(() => new RoutedCommand("Extend Selection", typeof(SyntaxEditor)));

        public ExtendSelectionAction()
            : base("Extend Selection")
        {
        }

        public static RoutedCommand Command
        {
            get { return LazyCommand.Value; }
        }

        public override void Execute(IEditorView view)
        {
            ExtendSelection(view);
        }
    }
}