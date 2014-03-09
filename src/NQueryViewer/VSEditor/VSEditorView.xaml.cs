using System;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery;
using NQuery.Authoring.VSEditorWpf.Document;
using NQuery.Authoring.VSEditorWpf.Selection;
using NQuery.Text;

namespace NQueryViewer.VSEditor
{
    internal sealed partial class VSEditorView : IVSEditorView
    {
        private readonly IWpfTextViewHost _textViewHost;
        private readonly INQueryDocument _document;
        private readonly INQuerySelectionProvider _selectionProvider;

        public VSEditorView(IWpfTextViewHost textViewHost, INQueryDocument document, INQuerySelectionProvider selectionProvider)
        {
            _textViewHost = textViewHost;
            _textViewHost.TextView.Caret.PositionChanged += CaretOnPositionChanged;
            _textViewHost.TextView.Selection.SelectionChanged += SelectionOnSelectionChanged;

            _document = document;
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
            _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;

            _selectionProvider = selectionProvider;

            InitializeComponent();

            EditorHost.Content = _textViewHost.HostControl;
            CaretPosition = GetEditorCaretPosition();
            Selection = GetEditorSelection();
            DocumentType = _document.DocumentType;
            DataContext = _document.DataContext;
            UpdateSyntaxTree();
            UpdateSemantiModel();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            var modifiers = e.KeyboardDevice.Modifiers;
            var key = e.Key;

            if (modifiers == ModifierKeys.Control && key == Key.W)
            {
                _selectionProvider.ExtendSelection();
                e.Handled = true;
            }
            else if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && key == Key.W)
            {
                _selectionProvider.ShrinkSelection();
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        public override void Focus()
        {
            var element = _textViewHost.TextView.VisualElement;
            element.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => element.Focus()));
        }

        protected override void OnCaretPositionChanged()
        {
            var snapshot = _textViewHost.TextView.TextSnapshot;
            var position = new SnapshotPoint(snapshot, CaretPosition);
            _textViewHost.TextView.Caret.MoveTo(position);
            _textViewHost.TextView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(position, position));
            base.OnCaretPositionChanged();
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            CaretPosition = GetEditorCaretPosition();
        }

        private int GetEditorCaretPosition()
        {
            return _textViewHost.TextView.Caret.Position.BufferPosition.Position;
        }

        protected override void OnSelectionChanged()
        {
            var snapshot = _textViewHost.TextView.TextSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, Selection.Start, Selection.Length);
            _textViewHost.TextView.Selection.Select(snapshotSpan, false);
            _textViewHost.TextView.ViewScroller.EnsureSpanVisible(snapshotSpan);
            base.OnSelectionChanged();
        }

        private void SelectionOnSelectionChanged(object sender, EventArgs e)
        {
            Selection = GetEditorSelection();
        }

        private TextSpan GetEditorSelection()
        {
            var selectedSpans = _textViewHost.TextView.Selection.SelectedSpans;
            if (!selectedSpans.Any())
                return new TextSpan(CaretPosition, 0);

            var span = selectedSpans.First();
            return new TextSpan(span.Start, span.Length);
        }

        protected override void OnDocumentTypeChanged()
        {
            _document.DocumentType = DocumentType;
            base.OnDocumentTypeChanged();
        }

        protected override void OnDataContextChanged()
        {
            _document.DataContext = DataContext;
            base.OnDataContextChanged();
        }

        private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs e)
        {
            DocumentType = _document.DocumentType;
            UpdateSyntaxTree();
        }

        private async void UpdateSyntaxTree()
        {
            SyntaxTree = await _document.GetSyntaxTreeAsync();
        }

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs e)
        {
            DataContext = _document.DataContext;
            UpdateSemantiModel();
        }

        private async void UpdateSemantiModel()
        {
            SemanticModel = await _document.GetSemanticModelAsync();
        }
    }
}
