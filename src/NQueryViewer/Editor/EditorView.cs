using System;
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

        public virtual int CaretPosition { get; set; }

        public virtual TextSpan Selection { get; set; }

        public new virtual void Focus()
        {
        }

        protected virtual void OnCaretPositionChanged()
        {
            var handler = CaretPositionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected virtual void OnSelectionChanged()
        {
            var handler = SelectionChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler CaretPositionChanged;

        public event EventHandler SelectionChanged;
    }
}