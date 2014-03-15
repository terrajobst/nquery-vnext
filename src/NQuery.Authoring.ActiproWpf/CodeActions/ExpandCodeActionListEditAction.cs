using System;
using System.Windows.Input;

using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.Implementation;

namespace NQuery.Authoring.ActiproWpf.CodeActions
{
    public class ExpandCodeActionListEditAction : EditActionBase
    {
        private static readonly Lazy<RoutedCommand> LazyCommand = new Lazy<RoutedCommand>(() => new RoutedCommand("Expand Code Action List", typeof(SyntaxEditor)));

        public static RoutedCommand Command
        {
            get { return LazyCommand.Value; }
        }

        public override void Execute(IEditorView view)
        {
            var controller = view.SyntaxEditor.Document.Language.GetService<ICodeActionGlyphController>();
            if (controller != null)
                controller.Expand();
        }
    }
}