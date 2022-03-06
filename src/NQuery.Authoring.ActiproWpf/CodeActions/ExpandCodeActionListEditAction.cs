using System.Windows.Input;

using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Implementation;

namespace NQuery.Authoring.ActiproWpf.CodeActions
{
    public class ExpandCodeActionListEditAction : EditActionBase
    {
        private static readonly Lazy<RoutedCommand> LazyCommand = new(() => new RoutedCommand("Expand Code Action List", typeof(SyntaxEditor)));

        public ExpandCodeActionListEditAction()
            : base("Expand Code Action List")
        {
        }

        public static RoutedCommand Command
        {
            get { return LazyCommand.Value; }
        }

        public override void Execute(IEditorView view)
        {
            var controller = view.SyntaxEditor.Document.Language.GetService<ICodeActionGlyphController>();
            controller?.Expand();
        }
    }
}