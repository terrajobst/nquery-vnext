using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Language;
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
        private DiagnosticsViewModel _diagnosticsViewModel;

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
            _editorHost.Content = _textViewHost.HostControl;

            _textViewHost.TextView.Caret.PositionChanged += CaretOnPositionChanged;

            var textBuffer = _textViewHost.TextView.TextBuffer;

            _document = DocumentManager.GetDocument(textBuffer);
            _document.DataContext = NorthwindDataContext.Create();
            _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;

            _selectionProvider = SelectionProviderService.GetSelectionProvider(_textViewHost.TextView);

            _diagnosticsViewModel = new DiagnosticsViewModel(_document);

            _diagnosticDataGrid.DataContext = _diagnosticsViewModel;

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

        private static NodeViewModel ToViewModel(SyntaxNodeOrToken nodeOrToken)
        {
            return nodeOrToken.IsNode
                       ? ToViewModel(nodeOrToken.AsNode())
                       : ToViewModel(nodeOrToken.AsToken());
        }

        private static NodeViewModel ToViewModel(SyntaxNode node)
        {
            var children = new List<NodeViewModel>();

            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsToken)
                    children.AddRange(child.AsToken().LeadingTrivia.Select(t => ToViewModel(t, true)));

                children.Add(ToViewModel(child));

                if (child.IsToken)
                    children.AddRange(child.AsToken().TrailingTrivia.Select(t => ToViewModel(t, false)));
            }

            return new NodeViewModel(node, children);
        }

        private static NodeViewModel ToViewModel(SyntaxToken token)
        {
            return new NodeViewModel(token, new List<NodeViewModel>());
        }

        private static NodeViewModel ToViewModel(SyntaxTrivia trivia, bool isLeading)
        {
            var children = new List<NodeViewModel>();
            
            if (trivia.Structure != null)
            {
                var structureViewModel = ToViewModel(trivia.Structure);
                children.Add(structureViewModel);
            }

            return new NodeViewModel(trivia, isLeading, children);
        }

        private enum NodeViewModelKind
        {
            Node,
            Token,
            Trivia
        }

        private sealed class NodeViewModel
        {
            public NodeViewModel(SyntaxToken data, IList<NodeViewModel> children)
            {
                Data = data;
                Title = data.Kind.ToString();
                NodeType = NodeViewModelKind.Token;
                Kind = data.Kind;
                ContextualKind = data.ContextualKind;
                Span = data.Span;
                FullSpan = data.FullSpan;
                IsMissing = data.IsMissing;
                UpdateChildren(children);
            }

            public NodeViewModel(SyntaxTrivia data, bool isLeading, IList<NodeViewModel> children)
            {
                Data = data;
                Title = string.Format("{0}:{1}", isLeading ? "L" : "T", data.Kind);
                NodeType = NodeViewModelKind.Trivia;
                Kind = data.Kind;
                ContextualKind = SyntaxKind.BadToken;
                Span = data.Span;
                FullSpan = data.Span;
                IsMissing = false;
                UpdateChildren(children);
            }

            public NodeViewModel(SyntaxNode data, IList<NodeViewModel> children)
            {
                Data = data;
                Title = data.Kind.ToString();
                NodeType = NodeViewModelKind.Node;
                Kind = data.Kind;
                ContextualKind = SyntaxKind.BadToken;
                Span = data.Span;
                FullSpan = data.FullSpan;
                IsMissing = data.IsMissing;
                UpdateChildren(children);
            }

            private void UpdateChildren(IList<NodeViewModel> children)
            {
                Children = new ReadOnlyCollection<NodeViewModel>(children);

                foreach (var nodeViewModel in children)
                    nodeViewModel.Parent = this;
            }

            public NodeViewModel Parent { get; private set; }

            public object Data { get; private set; }

            public string Title { get; private set; }

            public NodeViewModelKind NodeType { get; private set; }

            public SyntaxKind Kind { get; private set; }

            public SyntaxKind ContextualKind { get; private set; }

            public TextSpan Span { get; private set; }

            public TextSpan FullSpan { get; private set; }

            public bool IsMissing { get; set; }

            public ReadOnlyCollection<NodeViewModel> Children { get; private set; }

            public override string ToString()
            {
                return ContextualKind == SyntaxKind.BadToken
                           ? Kind.ToString()
                           : string.Format("{0} ({1})", Kind, ContextualKind);
            }
        }

        private sealed class DiagnosticsViewModel
        {
            private readonly INQueryDocument _document;

            public DiagnosticsViewModel(INQueryDocument document)
            {
                _document = document;
                _document.SyntaxTreeInvalidated += DocumentOnSyntaxTreeInvalidated;
                _document.SemanticModelInvalidated += DocumentOnSemanticModelInvalidated;
                Diagnostics = new ObservableCollection<DiagnosticViewModel>();
            }

            private void DocumentOnSemanticModelInvalidated(object sender, EventArgs e)
            {
                UpdateDiagnostics();
            }

            private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs e)
            {
                UpdateDiagnostics();
            }

            private async void UpdateDiagnostics()
            {
                var diagnostics = await GetDiagnosticsAsync();

                Diagnostics.Clear();
                foreach (var diagnostic in diagnostics)
                    Diagnostics.Add(diagnostic);
            }

            private async Task<IReadOnlyCollection<DiagnosticViewModel>> GetDiagnosticsAsync()
            {
                var syntaxTree = await _document.GetSyntaxTreeAsync();
                var semanticModel = await _document.GetSemanticModelAsync();
                var syntaxTreeDiagnostics = syntaxTree.GetDiagnostics();
                var semanticModelDiagnostics = semanticModel.GetDiagnostics();

                return (from d in syntaxTreeDiagnostics.Concat(semanticModelDiagnostics)
                       orderby d.Span.Start, d.Span.End
                       select new DiagnosticViewModel(d, syntaxTree.TextBuffer)).ToList();
            }

            public ObservableCollection<DiagnosticViewModel> Diagnostics { get; private set; }
        }

        private sealed class DiagnosticViewModel
        {
            public DiagnosticViewModel(Diagnostic diagnostic, TextBuffer textBuffer)
            {
                var textLocation = textBuffer.GetTextLocation(diagnostic.Span.Start);
                Diagnostic = diagnostic;
                Description = diagnostic.Message;
                Column = textLocation.Column + 1;
                Line = textLocation.Line + 1;
            }

            public Diagnostic Diagnostic { get; private set; }
            public string Description { get; private set; }
            public int Line { get; private set; }
            public int Column { get; private set; }
        }

        private async void UpdateTree()
        {
            var syntaxTree = await _document.GetSyntaxTreeAsync();
            var root = syntaxTree.Root;
            _treeView.ItemsSource = new[] { ToViewModel(root) };
            if (_textViewHost.HostControl.IsKeyboardFocusWithin)
                UpdateTreeExpansion();
        }

        private void UpdateTreeExpansion()
        {
            var position = _textViewHost.TextView.Caret.Position.BufferPosition.Position;
            var roots = _treeView.ItemsSource.OfType<NodeViewModel>();
            var node = FindViewModelNode(roots, position) ?? FindViewModelNode(roots, position - 1);
            if (node != null)
                _treeView.SelectNode(node, n => n.Parent, true);
        }

        private NodeViewModel FindViewModelNode(IEnumerable<NodeViewModel> roots, int position)
        {
            var nonTrivia = from r in roots
                            where r.NodeType != NodeViewModelKind.Trivia
                            select r;

            var skippedTokens = from r in roots
                                where r.Kind == SyntaxKind.SkippedTokensTrivia
                                from c in r.Children
                                select c;

            var children = nonTrivia.Concat(skippedTokens);

            foreach (var nodeViewModel in children)
            {
                if (nodeViewModel.Span.Contains(position))
                {
                    return nodeViewModel.Children.Any()
                               ? FindViewModelNode(nodeViewModel.Children, position)
                               : nodeViewModel;
                }
            }

            return null;
        }

        private void UpdateSelectedText()
        {
            var viewModel = _treeView.SelectedItem as NodeViewModel;
            if (viewModel == null)
                return;

            var span = _fullSpanHighlightingCheckBox.IsChecked == true
                           ? viewModel.FullSpan
                           : viewModel.Span;

            var snapshot = _textViewHost.TextView.TextBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, span.Start, span.Length);
            _textViewHost.TextView.Selection.Select(snapshotSpan, false);
            _textViewHost.TextView.ViewScroller.EnsureSpanVisible(snapshotSpan);
        }

        private void FullSpanHighlightingCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            UpdateSelectedText();
           
        }

        private void DocumentOnSyntaxTreeInvalidated(object sender, EventArgs eventArgs)
        {
            UpdateTree();
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_treeView.IsKeyboardFocusWithin)
                UpdateSelectedText();
        }

        private void CaretOnPositionChanged(object sender, CaretPositionChangedEventArgs caretPositionChangedEventArgs)
        {
            if (_textViewHost.HostControl.IsKeyboardFocusWithin)
                UpdateTreeExpansion();
        }

        private void DiagnosticDataGridOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var diagnosticViewModel = _diagnosticDataGrid.SelectedItems.OfType<DiagnosticViewModel>().FirstOrDefault();
            if (diagnosticViewModel == null)
                return;

            var span = diagnosticViewModel.Diagnostic.Span;
            var textView = _textViewHost.TextView;
            var snapshot = textView.TextBuffer.CurrentSnapshot;
            var snapshotSpan = new SnapshotSpan(snapshot, span.Start, span.Length);
            textView.Selection.Select(snapshotSpan, false);
        }
    }
}
