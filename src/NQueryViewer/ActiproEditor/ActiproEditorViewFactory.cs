using System.ComponentModel.Composition;

using NQueryViewer.Editor;

namespace NQueryViewer.ActiproEditor
{
    [Export(typeof(IEditorViewFactory))]
    [Export(typeof(IActiproEditorViewFactory))]
    internal sealed class ActiproEditorViewFactory : IActiproEditorViewFactory
    {
        public IActiproEditorView CreateEditorView()
        {
            return new ActiproEditorView();
        }

        IEditorView IEditorViewFactory.CreateEditorView()
        {
            return CreateEditorView();
        }

        public int Priority
        {
            get { return 2; }
        }

        public string DisplayName
        {
            get { return "Actipro SyntaxEditor"; }
        }
    }
}