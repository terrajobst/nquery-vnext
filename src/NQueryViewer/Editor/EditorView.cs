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
        private NQueryDocumentType _documentType;
        private DataContext _dataContext;
        private SyntaxTree _syntaxTree;
        private SemanticModel _semanticModel;

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

        public NQueryDocumentType DocumentType
        {
            get { return _documentType; }
            set
            {
                if (_documentType != value)
                {
                    _documentType = value;
                    OnDocumentTypeChanged();
                }
            }
        }

        public new DataContext DataContext
        {
            get { return _dataContext; }
            set
            {
                if (_dataContext != value)
                {
                    _dataContext = value;
                    OnDataContextChanged();
                }
            }
        }

        public SyntaxTree SyntaxTree
        {
            get { return _syntaxTree; }
            protected set
            {
                if (_syntaxTree != value)
                {
                    _syntaxTree = value;
                    OnSyntaxTreeChanged();
                }
            }
        }

        public SemanticModel SemanticModel
        {
            get { return _semanticModel; }
            protected set
            {
                if (_semanticModel != value)
                {
                    _semanticModel = value;
                    OnSemanticModelChanged();
                }
            }
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

        protected virtual void OnDocumentTypeChanged()
        {
            var handler = DocumentTypeChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected virtual void OnDataContextChanged()
        {
            var handler = DataContextChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected virtual void OnSyntaxTreeChanged()
        {
            var handler = SyntaxTreeChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected virtual void OnSemanticModelChanged()
        {
            var handler = SemanticModelChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public new virtual void Focus()
        {
        }

        public event EventHandler CaretPositionChanged;

        public event EventHandler SelectionChanged;

        public event EventHandler DocumentTypeChanged;

        public new event EventHandler DataContextChanged;

        public event EventHandler SyntaxTreeChanged;

        public event EventHandler SemanticModelChanged;
    }
}