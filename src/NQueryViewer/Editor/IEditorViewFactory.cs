using System;

namespace NQueryViewer.Editor
{
    public interface IEditorViewFactory
    {
        IEditorView CreateEditorView();

        int Priority { get; }
        string DisplayName { get; }
    }
}