using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using NQuery;
using NQuery.Data;
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
            get { return DocumentTabControl.SelectedContent as IEditorView; }
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

            var id = DocumentTabControl.Items.OfType<TabItem>().Select(t => t.Tag).OfType<int>().DefaultIfEmpty().Max() + 1;
            var tabItem = new TabItem
            {
                Header = string.Format("Query {0} [{1}]", id, editorViewFactory.DisplayName),
                Content = editorView.Element,
                Tag = id,
            };
            DocumentTabControl.Items.Add(tabItem);
            DocumentTabControl.SelectedItem = tabItem;
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

            DocumentTabControl.Items.RemoveAt(DocumentTabControl.SelectedIndex);
        }

        private async void RunQuery()
        {
            var editorView = CurrentEditorView;
            if (editorView == null)
                return;

            var semanticModel = editorView.SemanticModel;
            if (semanticModel == null)
                return;

            var syntaxTree = semanticModel.Compilation.SyntaxTree;
            var syntaxDiagnostics = syntaxTree.GetDiagnostics();
            var semanticModelDiagnostcics = semanticModel.GetDiagnostics();
            var diagnostics = syntaxDiagnostics.Concat(semanticModelDiagnostcics);
            if (diagnostics.Any())
                return;

            var query = semanticModel.Compilation.Compile();

            ExecutionTimeTextBlock.Text = "Running query...";

            var stopwatch = Stopwatch.StartNew();
            var dataTable = await Task.Run(() =>
            {
                using (var reader = query.CreateReader())
                    return reader.ExecuteDataTable();
            });
            var elapsed = stopwatch.Elapsed;

            DataGrid.ItemsSource = dataTable.DefaultView;
            BottomToolWindowTabControl.SelectedItem = ResultsTabItem;
            ExecutionTimeTextBlock.Text = string.Format("Completed in {0}", elapsed);
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
            else if (e.Key == Key.F5 && e.KeyboardDevice.Modifiers == ModifierKeys.None)
            {
                RunQuery();
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
                ShowPlanComboBox.ItemsSource = null;
            }
            else
            {
                var semanticModel = CurrentEditorView.SemanticModel;
                var optimizationSteps = semanticModel == null
                    ? ImmutableArray<ShowPlan>.Empty
                    : semanticModel.Compilation.GetShowPlanSteps().ToImmutableArray();
                ShowPlanComboBox.ItemsSource = optimizationSteps;
                ShowPlanComboBox.SelectedItem = optimizationSteps.LastOrDefault();
            }
        }

        private void TabControlOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                UpdateTree();
                UpdateDiagnostics();
                UpdateShowPlan();

                if (CurrentEditorView != null)
                    CurrentEditorView.Focus();
            }));

            DocumentTabControl.Visibility = DocumentTabControl.Items.Count > 0
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

        private void DataGridAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var dataView = (DataView) DataGrid.ItemsSource;
            var dataTable = dataView.Table;
            var dataColumn = dataTable.Columns[e.PropertyName];
            var columnName = string.IsNullOrWhiteSpace(dataColumn.Caption)
                                 ? "(No column name)"
                                 : dataColumn.Caption;
            var columnType = dataColumn.DataType.Name;
            var header = new StackPanel
                             {
                                 Orientation = Orientation.Vertical,
                                 Children =
                                     {
                                         new TextBlock(new Run(columnName)),
                                         new TextBlock(new Run(columnType))
                                             {
                                                 Foreground = Brushes.Gray
                                             }
                                     }
                             };
            e.Column.Header = header;
        }
    }
}
