using System;

namespace NQueryViewer.VSEditor
{
    public interface IVSEditorViewFactory
    {
        IVSEditorView CreateEditorView();
    }
}