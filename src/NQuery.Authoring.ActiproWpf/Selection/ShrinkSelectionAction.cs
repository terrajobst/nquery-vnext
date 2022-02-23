using System;
using System.Windows.Input;

using ActiproSoftware.Windows.Controls.SyntaxEditor;

namespace NQuery.Authoring.ActiproWpf.Selection
{
    public class ShrinkSelectionAction : SelectionAction
    {
        private static readonly Lazy<RoutedCommand> LazyCommand = new Lazy<RoutedCommand>(() => new RoutedCommand("Shrink Selection", typeof(SyntaxEditor)));

        public ShrinkSelectionAction()
            : base("Shrink Selection")
        {
        }

        public static RoutedCommand Command
        {
            get { return LazyCommand.Value; }
        }

        public override void Execute(IEditorView view)
        {
            ShrinkSelection(view);
        }
    }
}