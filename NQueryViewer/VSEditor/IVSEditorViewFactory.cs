using System;

using NQueryViewer.Editor;

namespace NQueryViewer.VSEditor
{
    public interface IVSEditorViewFactory : IEditorViewFactory
    {
        new IVSEditorView CreateEditorView();
    }
}