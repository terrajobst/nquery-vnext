using NQueryViewer.Editor;

namespace NQueryViewer.ActiproEditor
{
    public interface IActiproEditorViewFactory : IEditorViewFactory
    {
        new IActiproEditorView CreateEditorView();
    }
}