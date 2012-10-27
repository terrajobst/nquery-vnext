using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using NQuery.SampleData;

using NQueryViewer.Editor;
using NQueryViewer.VSEditor;

using TextBuffer = NQuery.Language.TextBuffer;

namespace NQueryViewer
{
    [Export(typeof(IMainWindowProvider))]
    internal sealed partial class MainWindow : IMainWindowProvider, IPartImportsSatisfiedNotification
    {
        [Import]
        public IVSEditorViewFactory VSEditorViewFactory { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        public Window Window
        {
            get { return this; }
        }

        private IEditorView CurrentEditorView
        {
            get { return TabControl.SelectedContent as IEditorView; }
        }

        public void OnImportsSatisfied()
        {
            NewEditor();
            UpdateTree();
        }

        private void NewEditor()
        {
            var editorView = VSEditorViewFactory.CreateEditorView();
            editorView.DataContext = NorthwindDataContext.Create();
            editorView.CaretPositionChanged += EditorViewOnCaretPositionChanged;
            editorView.SyntaxTreeChanged += EditorViewOnSyntaxTreeChanged;
            editorView.SemanticModelChanged += EditorViewOnSemanticModelChanged;
            
            var name = string.Format("Query {0} [{1}]", TabControl.Items.Count, editorView.GetType().Name);
            var tabItem = new TabItem
            {
                Header = name,
                Content = editorView.Element
            };
            TabControl.Items.Add(tabItem);
            TabControl.SelectedItem = tabItem;
            editorView.Focus();
        }

        private void CloseEditor()
        {
            var editorView = CurrentEditorView;
            if (editorView == null)
                return;

            editorView.CaretPositionChanged -= EditorViewOnCaretPositionChanged;
            editorView.SyntaxTreeChanged -= EditorViewOnSyntaxTreeChanged;
            editorView.SemanticModelChanged -= EditorViewOnSemanticModelChanged;

            TabControl.Items.RemoveAt(TabControl.SelectedIndex);
        }

        private void UpdateTree()
        {
            SyntaxTreeVisualizer.SyntaxTree = CurrentEditorView == null ? null : CurrentEditorView.SyntaxTree;
            UpdateTreeExpansion();
        }

        private void UpdateTreeExpansion()
        {
            if (CurrentEditorView != null)
                SyntaxTreeVisualizer.SelectNode(CurrentEditorView.CaretPosition);
        }

        private void UpdateSelectedText()
        {
            var span = FullSpanHighlightingCheckBox.IsChecked == true
                              ? SyntaxTreeVisualizer.SelectedFullSpan
                              : SyntaxTreeVisualizer.SelectedSpan;
            if (span == null)
                return;

            if (CurrentEditorView != null)
                CurrentEditorView.Selection = span.Value;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.N && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                NewEditor();
                e.Handled = true;
            }
            else if (e.Key == Key.F4 && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                CloseEditor();
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        private void FullSpanHighlightingCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            UpdateSelectedText();
        }

        private void EditorViewOnSyntaxTreeChanged(object sender, EventArgs eventArgs)
        {
            UpdateTree();
        }

        private void EditorViewOnSemanticModelChanged(object sender, EventArgs eventArgs)
        {
            UpdateDiagnostics();
        }

        private void UpdateDiagnostics()
        {
            if (CurrentEditorView == null)
            {
                DiagnosticGrid.UpdateGrid(null, null);
            }
            else
            {
                var semanticModel = CurrentEditorView.SemanticModel;
                var syntaxTree = semanticModel.Compilation.SyntaxTree;
                var syntaxTreeDiagnostics = syntaxTree.GetDiagnostics();
                var semanticModelDiagnostics = semanticModel.GetDiagnostics();
                var diagnostics = syntaxTreeDiagnostics.Concat(semanticModelDiagnostics);
                DiagnosticGrid.UpdateGrid(diagnostics, syntaxTree.TextBuffer);
            }
        }

        private void TabControlOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                UpdateTree();
                UpdateDiagnostics();

                if (CurrentEditorView != null)
                    CurrentEditorView.Focus();
            }));

            TabControl.Visibility = TabControl.Items.Count > 0
                                        ? Visibility.Visible
                                        : Visibility.Collapsed;
        }

        private void SyntaxTreeVisualizerSelectedNodeChanged(object sender, EventArgs e)
        {
            if (SyntaxTreeVisualizer.IsKeyboardFocusWithin)
                UpdateSelectedText();
        }

        private void EditorViewOnCaretPositionChanged(object sender, EventArgs e)
        {
            if (CurrentEditorView != null && CurrentEditorView.Element.IsKeyboardFocusWithin)
                UpdateTreeExpansion();
        }

        private void DiagnosticGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var diagnostic = DiagnosticGrid.SelectedDiagnostic;
            if (diagnostic == null)
                return;

            if (CurrentEditorView != null)
                CurrentEditorView.Selection = diagnostic.Span;
        }
    }
}
