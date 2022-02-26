using System.Windows;

using NQuery.Authoring;
using NQuery.Text;

namespace NQueryViewer.Editor
{
    public interface IEditorView
    {
        UIElement Element { get; }
        int CaretPosition { get; set; }
        TextSpan Selection { get; set; }
        Workspace Workspace { get; }
        double ZoomLevel { get; set; }

        void Focus();
        DocumentView GetDocumentView();

        event EventHandler CaretPositionChanged;
        event EventHandler SelectionChanged;
        event EventHandler ZoomLevelChanged;
    }
}