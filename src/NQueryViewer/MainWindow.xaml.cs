using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.IO;
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
using NQuery.Syntax;
using NQuery.Text;

using NQueryViewer.Editor;

using TextChange = NQuery.Text.TextChange;

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

            editorView.CaretPositionChanged += EditorViewOnCaretPositionChanged;
            editorView.Workspace.DataContext = NorthwindDataContext.Instance;
            editorView.Workspace.CurrentDocumentChanged += WorkspaceOnCurrentDocumentChanged;

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
            editorView.Workspace.CurrentDocumentChanged -= WorkspaceOnCurrentDocumentChanged;

            DocumentTabControl.Items.RemoveAt(DocumentTabControl.SelectedIndex);
        }

        private async void RunQuery()
        {
            var editorView = CurrentEditorView;
            if (editorView == null)
                return;

            var document = editorView.Workspace.CurrentDocument;
            var semanticModel = await document.GetSemanticModelAsync();
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

        private async void UpdateTree()
        {
            SyntaxTreeVisualizer.SyntaxTree = CurrentEditorView == null
                ? null
                : await CurrentEditorView.Workspace.CurrentDocument.GetSyntaxTreeAsync();
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

        private void WorkspaceOnCurrentDocumentChanged(object sender, EventArgs e)
        {
            UpdateTree();
            UpdateDiagnostics();
            UpdateShowPlan();
        }

        private async void UpdateDiagnostics()
        {
            if (CurrentEditorView == null)
            {
                DiagnosticGrid.UpdateGrid(null, null);
            }
            else
            {
                var document = CurrentEditorView.Workspace.CurrentDocument;
                var semanticModel = await document.GetSemanticModelAsync();
                var syntaxTree = semanticModel == null
                                     ? await document.GetSyntaxTreeAsync()
                                     : semanticModel.Compilation.SyntaxTree;
                var syntaxTreeDiagnostics = syntaxTree == null
                                                ? Enumerable.Empty<Diagnostic>()
                                                : syntaxTree.GetDiagnostics();
                var semanticModelDiagnostics = semanticModel == null
                                                   ? Enumerable.Empty<Diagnostic>()
                                                   : semanticModel.GetDiagnostics();
                var diagnostics = syntaxTreeDiagnostics.Concat(semanticModelDiagnostics);
                var text = syntaxTree == null
                                     ? null
                                     : syntaxTree.Text;
                DiagnosticGrid.UpdateGrid(diagnostics, text);
            }
        }

        private async void UpdateShowPlan()
        {
            if (CurrentEditorView == null)
            {
                ShowPlanComboBox.ItemsSource = null;
            }
            else
            {
                var document = CurrentEditorView.Workspace.CurrentDocument;
                var semanticModel = await document.GetSemanticModelAsync();
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

        private void MenuItem_File_Exist_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void MenuItem_Tools_GenerateParserTestToClipBoard_OnClick(object sender, RoutedEventArgs e)
        {
            if (CurrentEditorView == null)
                return;

            var document = CurrentEditorView.Workspace.CurrentDocument;
            var textSpan = CurrentEditorView.Selection;

            if (textSpan.Length == 0)
                textSpan = new TextSpan(0, document.Text.Length);

            var syntaxTree = await document.GetSyntaxTreeAsync();
            var node = syntaxTree.Root.DescendantNodes()
                                      .Last(n => n.Span.Contains(textSpan));
            var nodeOrTokens = syntaxTree.Root.Root.DescendantNodesAndTokensAndSelf(true)
                                                   .Where(n => textSpan.Contains(n.Span));

            var isExpression = node is ExpressionSyntax;

            string text;

            if (node.Span == textSpan)
            {
                text = document.Text.GetText(node.Span);
            }
            else if (syntaxTree.Root.Root.Span == textSpan)
            {
                text = document.Text.GetText();
            }
            else
            {
                text = document.Text.WithChanges(
                    TextChange.ForInsertion(textSpan.Start, "{"),
                    TextChange.ForInsertion(textSpan.End, "}")
                ).GetText();
            }

            using (var stringWriter = new StringWriter())
            {
                using (var writer = new IndentedTextWriter(stringWriter))
                {
                    writer.Indent = 2;

                    var sb = writer;
                    sb.WriteLine("[Fact]");
                    sb.WriteLine("public void Parser_Parse_{0}()", node.GetType().Name.Substring(0, node.GetType().Name.Length - "Syntax".Length));
                    sb.WriteLine("{");

                    writer.Indent++;

                    sb.WriteLine("const string text = @\"");

                    using (var stringReader = new StringReader(text))
                    {
                        writer.Indent++;

                        string line;
                        while ((line = stringReader.ReadLine()) != null)
                        {
                            sb.WriteLine(line.Replace("\"", "\"\""));
                        }

                        writer.Indent--;
                    }

                    var method = isExpression ? "ForExpression" : "ForQuery";

                    sb.WriteLine("\";");
                    sb.WriteLine();
                    sb.WriteLine("using (var enumerator = AssertingEnumerator.{0}(text))", method);
                    sb.WriteLine("{");

                    writer.Indent++;

                    foreach (var nodesOrToken in nodeOrTokens)
                    {
                        if (nodesOrToken.IsNode)
                        {
                            var missingModifier = nodesOrToken.IsMissing ? "Missing" : "";
                            sb.WriteLine("enumerator.AssertNode{0}(SyntaxKind.{1});", missingModifier, nodesOrToken.Kind);
                        }
                        else
                        {
                            var token = nodesOrToken.AsToken();
                            var tokenText = token.Text.Replace("\"", "\"\"");

                            if (token.IsMissing)
                            {
                                sb.WriteLine("enumerator.AssertTokenMissing(SyntaxKind.{0});", nodesOrToken.Kind);
                            }
                            else
                            {
                                sb.WriteLine("enumerator.AssertToken(SyntaxKind.{0}, @\"{1}\");", nodesOrToken.Kind, tokenText);
                            }

                            foreach (var diagnostic in token.Diagnostics)
                            {
                                sb.WriteLine("enumerator.AssertDiagnostic(DiagnosticId.{0}, @\"{1}\");", diagnostic.DiagnosticId, diagnostic.Message);
                            }
                        }
                    }

                    writer.Indent--;
                    sb.WriteLine("}");

                    writer.Indent--;
                    sb.WriteLine("}");
                }

                Clipboard.SetText(stringWriter.ToString());
            }
        }
    }
}
