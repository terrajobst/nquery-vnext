using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using NQuery;
using NQuery.Data.Samples;

using NQueryViewer.Editor;

namespace NQueryViewer
{
    [Export(typeof(IMainWindowProvider))]
    internal sealed partial class MainWindow : IMainWindowProvider, IPartImportsSatisfiedNotification
    {
        [ImportMany]
        public IEnumerable<IEditorViewFactory> EditorViewFactories { get; set; }

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
            foreach (var editorViewFactory in EditorViewFactories.OrderBy(e => e.Priority))
                EditorFactoryComboBox.Items.Add(editorViewFactory);

            if (EditorFactoryComboBox.Items.Count > 0)
                EditorFactoryComboBox.SelectedIndex = 0;

            NewEditor();
            UpdateTree();
        }

        private void NewEditor()
        {
            var editorViewFactory = EditorFactoryComboBox.SelectedItem as IEditorViewFactory;
            if (editorViewFactory == null)
                return;

            var editorView = editorViewFactory.CreateEditorView();
            if (editorView == null)
                return;

            editorView.DataContext = DataContextFactory.CreateNorthwind();
            editorView.CaretPositionChanged += EditorViewOnCaretPositionChanged;
            editorView.SyntaxTreeChanged += EditorViewOnSyntaxTreeChanged;
            editorView.SemanticModelChanged += EditorViewOnSemanticModelChanged;

            var id = TabControl.Items.OfType<TabItem>().Select(t => t.Tag).OfType<int>().DefaultIfEmpty().Max() + 1;
            var tabItem = new TabItem
            {
                Header = string.Format("Query {0} [{1}]", id, editorViewFactory.DisplayName),
                Content = editorView.Element,
                Tag = id,
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
            UpdateShowPlan();
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
                var syntaxTree = semanticModel == null
                                     ? CurrentEditorView.SyntaxTree
                                     : semanticModel.Compilation.SyntaxTree;
                var syntaxTreeDiagnostics = syntaxTree == null
                                                ? Enumerable.Empty<Diagnostic>()
                                                : syntaxTree.GetDiagnostics();
                var semanticModelDiagnostics = semanticModel == null
                                                   ? Enumerable.Empty<Diagnostic>()
                                                   : semanticModel.GetDiagnostics();
                var diagnostics = syntaxTreeDiagnostics.Concat(semanticModelDiagnostics);
                var textBuffer = syntaxTree == null
                                     ? null
                                     : syntaxTree.TextBuffer;
                DiagnosticGrid.UpdateGrid(diagnostics, textBuffer);
            }
        }

        private void UpdateShowPlan()
        {
            if (CurrentEditorView == null)
            {
                _showPlanView.ShowPlan = null;
            }
            else
            {
                var semanticModel = CurrentEditorView.SemanticModel;
                var syntaxTree = semanticModel.Compilation.SyntaxTree;
                var hasDiagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).Any();
                if (hasDiagnostics)
                    return;

                _showPlanView.ShowPlan = semanticModel.Compilation.GetShowPlan();
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
