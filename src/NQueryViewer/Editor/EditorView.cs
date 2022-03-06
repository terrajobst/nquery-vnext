using System.Windows;
using System.Windows.Controls;

using NQuery.Authoring;
using NQuery.Text;

namespace NQueryViewer.Editor
{
    public class EditorView : UserControl, IEditorView
    {
        public UIElement Element
        {
            get { return this; }
        }

        public virtual Workspace Workspace
        {
            get { return null; }
        }

        public virtual double ZoomLevel { get; set; }

        public virtual int CaretPosition { get; set; }

        public virtual TextSpan Selection { get; set; }

        public new virtual void Focus()
        {
        }

        public virtual DocumentView GetDocumentView()
        {
            return null;
        }

        protected virtual void OnCaretPositionChanged()
        {
            var handler = CaretPositionChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnSelectionChanged()
        {
            var handler = SelectionChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnZoomLevelChanged()
        {
            var handler = ZoomLevelChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CaretPositionChanged;

        public event EventHandler SelectionChanged;

        public event EventHandler ZoomLevelChanged;
    }
}