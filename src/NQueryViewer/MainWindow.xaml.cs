using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using NQuery;
using NQuery.Authoring;
using NQuery.Data;

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

        private IEnumerable<IEditorView> EditorViews
        {
            get { return DocumentTabControl.Items.OfType<TabItem>().Select(t => t.Content).OfType<IEditorView>(); }
        }

        public void OnImportsSatisfied()
        {
            foreach (var editorViewFactory in EditorViewFactories.OrderBy(e => e.Priority))
            {
                var menuItem = new MenuItem();
                menuItem.Header = $"New {editorViewFactory.DisplayName}";
                menuItem.Tag = editorViewFactory;
                menuItem.Click += delegate { NewEditor(editorViewFactory); };

                FileMenuItem.Items.Insert(FileMenuItem.Items.IndexOf(FileNewSeparator), menuItem);
            }

            NewEditor();
            UpdateTree();
        }

        private void NewEditor()
        {
            var editorViewFactory = EditorViewFactories.OrderBy(e => e.Priority).FirstOrDefault();
            NewEditor(editorViewFactory);
        }

        private void NewEditor(IEditorViewFactory editorViewFactory)
        {
            var editorView = editorViewFactory?.CreateEditorView();
            if (editorView is null)
                return;

            editorView.CaretPositionChanged += EditorViewOnCaretPositionChanged;
            editorView.ZoomLevelChanged += EditorViewOnZoomLevelChanged;
            editorView.Workspace.DataContext = NorthwindDataContext.Instance;
            editorView.Workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;

            if (CurrentEditorView is not null)
                editorView.ZoomLevel = CurrentEditorView.ZoomLevel;

            var id = DocumentTabControl.Items.OfType<TabItem>().Select(t => t.Tag).OfType<int>().DefaultIfEmpty().Max() + 1;
            var tabItem = new TabItem
            {
                Header = $"Query {id} [{editorViewFactory.DisplayName}]",
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
            if (editorView is null)
                return;

            editorView.CaretPositionChanged -= EditorViewOnCaretPositionChanged;
            editorView.ZoomLevelChanged -= EditorViewOnZoomLevelChanged;
            editorView.Workspace.CurrentDocumentChanged -= WorkspaceOnCurrentDocumentChanged;

            DocumentTabControl.Items.RemoveAt(DocumentTabControl.SelectedIndex);
        }

        private async void ExecuteQuery()
        {
            var editorView = CurrentEditorView;
            if (editorView is null)
                return;

            var document = editorView.Workspace.CurrentDocument;
            var semanticModel = await document.GetSemanticModelAsync();
            if (semanticModel is null)
                return;

            var syntaxTree = semanticModel.SyntaxTree;
            var syntaxDiagnostics = syntaxTree.GetDiagnostics();
            var semanticModelDiagnostics = semanticModel.GetDiagnostics();
            var diagnostics = syntaxDiagnostics.Concat(semanticModelDiagnostics);
            if (diagnostics.Any())
            {
                BottomToolWindowTabControl.SelectedItem = ErrorListTabItem;
                return;
            }

            var query = semanticModel.Compilation.Compile();

            ExecutionTimeTextBlock.Text = "Running query...";

            var stopwatch = Stopwatch.StartNew();

            DataTable dataTable = null;
            Exception exception = null;

            try
            {
                dataTable = await Task.Run(() =>
                {
                    using (var reader = query.CreateReader())
                        return reader.ExecuteDataTable();
                });
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            var elapsed = stopwatch.Elapsed;

            DataGrid.ItemsSource = dataTable?.DefaultView;
            BottomToolWindowTabControl.SelectedItem = ResultsTabItem;
            ExecutionTimeTextBlock.Text = $"Completed in {elapsed}";

            if (exception is not null)
                MessageBox.Show(exception.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ExplainQuery()
        {
            BottomToolWindowTabControl.SelectedItem = ExecutionPlanTabItem;
        }

        private void SetDocumentKind(DocumentKind kind)
        {
            if (CurrentEditorView is not null)
                CurrentEditorView.Workspace.DocumentKind = kind;
        }

        private DocumentKind GetDocumentKind()
        {
            return CurrentEditorView?.Workspace.DocumentKind ?? DocumentKind.Query;
        }

        private void UpdateEditorState()
        {
            UpdateDocumentState();

            CurrentEditorView?.Focus();
        }

        private void UpdateDocumentState()
        {
            UpdateTree();
            UpdateDiagnostics();
            UpdateShowPlan();
            UpdateDocumentKind();
        }

        private void UpdateDocumentKind()
        {
            var currentKind = GetDocumentKind();
            QueryModeQueryMenuItem.IsChecked = currentKind == DocumentKind.Query;
            QueryModeExpressionMenuItem.IsChecked = currentKind == DocumentKind.Expression;
        }

        private async void UpdateTree()
        {
            var isVisible = ToolsViewSyntaxMenuItem.IsChecked;

            var visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            SyntaxTreeVisualizer.Visibility = visibility;
            SyntaxTreeVisualizerGridSplitter.Visibility = visibility;

            SyntaxTreeVisualizerColumn.Width = isVisible ? new GridLength(1, GridUnitType.Star) : new GridLength();
            SyntaxTreeVisualizerGridSplitterColumn.Width = isVisible ? new GridLength(3) : new GridLength();

            if (!isVisible)
            {
                SyntaxTreeVisualizer.SyntaxTree = SyntaxTree.Empty;
                return;
            }

            SyntaxTreeVisualizer.SyntaxTree = CurrentEditorView is null
                ? null
                : await CurrentEditorView.Workspace.CurrentDocument.GetSyntaxTreeAsync();
            UpdateTreeExpansion();
        }

        private void UpdateTreeExpansion()
        {
            if (!ToolsViewSyntaxMenuItem.IsChecked)
                return;

            if (CurrentEditorView is not null)
                SyntaxTreeVisualizer.SelectNode(CurrentEditorView.CaretPosition);
        }

        private void UpdateSelectedText()
        {
            var span = SyntaxTreeVisualizer.SelectedSpan;
            if (span is null)
                return;

            if (CurrentEditorView is not null)
                CurrentEditorView.Selection = span.Value;
        }

        private async void UpdateDiagnostics()
        {
            if (CurrentEditorView is null)
            {
                DiagnosticGrid.UpdateGrid(null, null);
            }
            else
            {
                var document = CurrentEditorView.Workspace.CurrentDocument;
                var semanticModel = await document.GetSemanticModelAsync();
                var syntaxTree = semanticModel is null
                    ? await document.GetSyntaxTreeAsync()
                    : semanticModel.SyntaxTree;
                var syntaxTreeDiagnostics = syntaxTree is null
                    ? Enumerable.Empty<Diagnostic>()
                    : syntaxTree.GetDiagnostics();
                var semanticModelDiagnostics = semanticModel is null
                    ? Enumerable.Empty<Diagnostic>()
                    : semanticModel.GetDiagnostics();
                var diagnostics = syntaxTreeDiagnostics.Concat(semanticModelDiagnostics);
                var text = syntaxTree?.Text;
                DiagnosticGrid.UpdateGrid(diagnostics, text);
            }
        }

        private async void UpdateShowPlan()
        {
            if (CurrentEditorView is null)
            {
                ShowPlanComboBox.ItemsSource = null;
            }
            else
            {
                var document = CurrentEditorView.Workspace.CurrentDocument;
                var semanticModel = await document.GetSemanticModelAsync();
                var optimizationSteps = semanticModel is null
                    ? ImmutableArray<ShowPlan>.Empty
                    : semanticModel.Compilation.GetShowPlanSteps().ToImmutableArray();
                ShowPlanComboBox.ItemsSource = optimizationSteps;
                ShowPlanComboBox.SelectedItem = optimizationSteps.LastOrDefault();
            }
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
                ExecuteQuery();
                e.Handled = true;
            }
            else if (e.Key == Key.L && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                ExplainQuery();
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        private void FileExitMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void QueryExecuteMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExecuteQuery();
        }

        private void QueryExplainMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ExplainQuery();
        }

        private void QueryModeQueryMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            SetDocumentKind(DocumentKind.Query);
        }

        private void QueryModeExpressionMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            SetDocumentKind(DocumentKind.Expression);
        }

        private void ToolsViewSyntaxMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ToolsViewSyntaxMenuItem.IsChecked = !ToolsViewSyntaxMenuItem.IsChecked;

            UpdateTree();
        }

        private async void ToolsGenerateParserTestMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var documentView = CurrentEditorView?.GetDocumentView();
            if (documentView is null)
                return;

            var test = await documentView.GenerateParserTest();
            Clipboard.SetText(test);
        }

        private void SyntaxTreeVisualizerSelectedNodeChanged(object sender, EventArgs e)
        {
            if (SyntaxTreeVisualizer.IsKeyboardFocusWithin)
                UpdateSelectedText();
        }

        private void TabControlOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(UpdateEditorState));

            DocumentTabControl.Visibility = DocumentTabControl.Items.Count > 0
                                        ? Visibility.Visible
                                        : Visibility.Collapsed;
        }

        private void EditorViewOnCaretPositionChanged(object sender, EventArgs e)
        {
            if (CurrentEditorView is not null && CurrentEditorView.Element.IsKeyboardFocusWithin)
                UpdateTreeExpansion();
        }

        private void EditorViewOnZoomLevelChanged(object sender, EventArgs e)
        {
            if (sender is not IEditorView changedView)
                return;

            var newZoomLevel = changedView.ZoomLevel;

            foreach (var editorView in EditorViews)
                editorView.ZoomLevel = newZoomLevel;

            var percent = newZoomLevel / 100;

            BottomToolWindowTabControl.FontSize = FontSize * percent;
        }

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateDocumentState();
        }

        private void DiagnosticGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var diagnostic = DiagnosticGrid.SelectedDiagnostic;
            if (diagnostic is null)
                return;

            if (CurrentEditorView is not null)
                CurrentEditorView.Selection = diagnostic.Span;
        }

        private void DataGridAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var dataView = (DataView)DataGrid.ItemsSource;
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

            // For some reason, the WPF DataGrid doesn't escape property names containing
            // a period. This results queries like this:
            //
            //      SELECT  e.FirstName [A.B]
            //      FROM    Employees e
            //
            // to display nothing because the binding path A.B can't be resolved.
            //
            // To fix this, we force the path to be escaped.

            if (e.Column is DataGridBoundColumn gridColumn)
                gridColumn.Binding = new Binding("[" + e.PropertyName + "]");
        }
    }
}