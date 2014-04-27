using System;
using System.Windows;
using System.Windows.Controls;

using NQuery;
using NQuery.Authoring.Document;
using NQuery.Text;

namespace NQueryViewer.Editor
{
    public class EditorView : UserControl, IEditorView
    {
        private int _caretPosition;
        private TextSpan _selection;

        public UIElement Element
        {
            get { return this; }
        }

        public int CaretPosition
        {
            get { return _caretPosition; }
            set
            {
                if (_caretPosition != value)
                {
                    _caretPosition = value;
                    OnCaretPositionChanged();
                }
            }
        }

        public TextSpan Selection
        {
            get { return _selection; }
            set
            {
                if (_selection != value)
                {
                    _selection = value;
                    OnSelectionChanged();
                }
            }
        }

        public virtual NQueryDocument Document
        {
            get { return null; }
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

        public new virtual void Focus()
        {
        }

        public event EventHandler CaretPositionChanged;

        public event EventHandler SelectionChanged;
    }
}