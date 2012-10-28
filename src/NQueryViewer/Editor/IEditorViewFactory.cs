using System;

namespace NQueryViewer.Editor
{
    public interface IEditorViewFactory
    {
        IEditorView CreateEditorView();

        string DisplayName { get; }
    }
}