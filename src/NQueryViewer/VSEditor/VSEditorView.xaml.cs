using System;
using System.Windows.Input;
using System.Windows.Threading;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Authoring;
using NQuery.Authoring.VSEditorWpf;
using NQuery.Authoring.VSEditorWpf.Selection;
using NQuery.Text;

namespace NQueryViewer.VSEditor
{
    internal sealed partial class VSEditorView : IVSEditorView
    {
        private readonly Workspace _workspace;
        private readonly IWpfTextViewHost _textViewHost;
        private readonly INQuerySelectionProvider _selectionProvider;

        public VSEditorView(Workspace workspace, IWpfTextViewHost textViewHost, INQuerySelectionProvider selectionProvider)
        {
            _workspace = workspace;
            _textViewHost = textViewHost;
            _textViewHost.TextView.Selection.SelectionChanged += SelectionOnSelectionChanged;

            _selectionProvider = selectionProvider;

            InitializeComponent();

            EditorHost.Content = _textViewHost.HostControl;
        }

        private void SelectionOnSelectionChanged(object sender, EventArgs e)
        {
            OnCaretPositionChanged();
            OnSelectionChanged();
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

        public override DocumentView GetDocumentView()
        {
            return _textViewHost.TextView.GetDocumentView();
        }

        private int GetCaretPosition()
        {
            return _textViewHost.TextView.Caret.Position.BufferPosition.Position;
        }

        private void SetCaretPosition(int caretPosition)
        {
            var snapshot = _textViewHost.TextView.TextSnapshot;
            var position = new SnapshotPoint(snapshot, caretPosition);
            _textViewHost.TextView.Caret.MoveTo(position);
            _textViewHost.TextView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(position, position));
        }

        private void SetSelection(TextSpan selection)
        {
            var snapshot = _textViewHost.TextView.TextSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, selection.Start, selection.Length);
            _textViewHost.TextView.Selection.Select(snapshotSpan, false);
            _textViewHost.TextView.ViewScroller.EnsureSpanVisible(snapshotSpan);
        }

        private TextSpan GetSelection()
        {
            var start = _textViewHost.TextView.Selection.Start.Position.Position;
            var end = _textViewHost.TextView.Selection.End.Position.Position;
            return TextSpan.FromBounds(start, end);
        }

        public override Workspace Workspace
        {
            get { return _workspace; }
        }

        public override int CaretPosition
        {
            get { return GetCaretPosition(); }
            set {  SetCaretPosition(value); }
        }

        public override TextSpan Selection
        {
            get { return GetSelection(); }
            set { SetSelection(value); }
        }
    }
}
