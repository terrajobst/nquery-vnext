using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Language.VSEditor.Document;
using NQuery.Language.VSEditor.Selection;
using NQuery.SampleData;

using NQueryViewer.Helpers;

using TextBuffer = NQuery.Language.TextBuffer;

namespace NQueryViewer
{
    [Export(typeof(IMainWindowProvider))]
    internal sealed partial class MainWindow : IMainWindowProvider, IPartImportsSatisfiedNotification
    {
        private IWpfTextViewHost _textViewHost;
        private INQueryDocument _document;
        private INQuerySelectionProvider _selectionProvider;

        [Import]
        public TextViewFactory TextViewFactory { get; set; }

        [Import]
        public INQueryDocumentManager DocumentManager { get; set; }

        [Import]
        public INQuerySelectionProviderService SelectionProviderService { get; set; }

        public Window Window
        {
            get { return this; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        public void OnImportsSatisfied()
        {
            _textViewHost = TextViewFactory.CreateTextViewHost();
            EditorHost.Content = _textViewHost.HostControl;

            _textViewHost.TextView.Caret.PositionChanged += CaretOnPositionChanged;

            var textBuffer = _textViewHost.TextView.TextBuffer;

            _document = DocumentManager.GetDocument(textBuffer);
            _document.DataContext = NorthwindDataContext.Create();
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
            _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;

            _selectionProvider = SelectionProviderService.GetSelectionProvider(_textViewHost.TextView);

            UpdateTree();
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

        private async void UpdateTree()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            SyntaxTreeVisualizer.SyntaxTree = syntaxTree;
            UpdateTreeExpansion();
        }

        private void UpdateTreeExpansion()
        {
            var position = _textViewHost.TextView.Caret.Position.BufferPosition.Position;
            SyntaxTreeVisualizer.SelectNode(position);
        }

        private void UpdateSelectedText()
        {
            var spanOpt = FullSpanHighlightingCheckBox.IsChecked == true
                              ? SyntaxTreeVisualizer.SelectedFullSpan
                              : SyntaxTreeVisualizer.SelectedSpan;
            if (spanOpt == null)
                return;

            var span = spanOpt.Value;
            var snapshot = _textViewHost.TextView.TextBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, span.Start, span.Length);
            _textViewHost.TextView.Selection.Select(snapshotSpan, false);
            _textViewHost.TextView.ViewScroller.EnsureSpanVisible(snapshotSpan);
        }

        private void FullSpanHighlightingCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            UpdateSelectedText();
           
        }

        private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs e)
        {
            UpdateTree();
        }

        private void DocumentOnSemanticModelInvalidated(object sender, EventArgs e)
        {
            UpdateDiagnostics();
        }

        private async void UpdateDiagnostics()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var semanticModel = await _document.GetSemanticModelAsync();
            var syntaxTreeDiagnostics = syntaxTree.GetDiagnostics();
            var semanticModelDiagnostics = semanticModel.GetDiagnostics();
            var diagnostics = syntaxTreeDiagnostics.Concat(semanticModelDiagnostics);
            DiagnosticGrid.UpdateGrid(diagnostics, syntaxTree.TextBuffer);
        }

        private void SyntaxTreeVisualizerSelectedNodeChanged(object sender, EventArgs e)
        {
            if (SyntaxTreeVisualizer.IsKeyboardFocusWithin)
                UpdateSelectedText();
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs e)
        {
            if (_textViewHost.HostControl.IsKeyboardFocusWithin)
                UpdateTreeExpansion();
        }

        private void DiagnosticGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var diagnostic = DiagnosticGrid.SelectedDiagnostic;
            if (diagnostic == null)
                return;

            var span = diagnostic.Span;
            var textView = _textViewHost.TextView;
            var snapshot = textView.TextBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, span.Start, span.Length);
            textView.Selection.Select(snapshotSpan, false);
        }
    }
}
