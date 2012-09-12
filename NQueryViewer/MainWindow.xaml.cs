using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

using NQuery.Language;
using NQuery.Language.Runtime;
using NQuery.Language.Symbols;
using NQuery.Language.VSEditor;

using NQueryViewer.Helpers;

using TextBuffer = NQuery.Language.TextBuffer;

namespace NQueryViewer
{
    [Export(typeof(IMainWindowProvider))]
    internal sealed partial class MainWindow : IMainWindowProvider, IPartImportsSatisfiedNotification
    {
        private IWpfTextViewHost _textViewHost;
        private INQuerySyntaxTreeManager _syntaxTreeManager;
        private INQuerySemanticModelManager _semanticModelManager;
        private INQuerySelectionProvider _selectionProvider;
        private DiagnosticsViewModel _diagnosticsViewModel;

        [Import]
        public TextViewFactory TextViewFactory { get; set; }

        [Import]
        public INQuerySyntaxTreeManagerService SyntaxTreeManagerService { get; set; }

        [Import]
        public INQuerySemanticModelManagerService SemanticModelManagerService { get; set; }

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

            _syntaxTreeManager = SyntaxTreeManagerService.GetSyntaxTreeManager(textBuffer);
            _syntaxTreeManager.SyntaxTreeChanged += SyntaxTreeManagerOnSyntaxTreeChanged;
            _semanticModelManager = SemanticModelManagerService.GetSemanticModelManager(textBuffer);

            _semanticModelManager.Compilation = _semanticModelManager.Compilation.WithDataContext(GetDataContext());

            _selectionProvider = SelectionProviderService.GetSelectionProvider(_textViewHost.TextView);

            _diagnosticsViewModel = new DiagnosticsViewModel(_syntaxTreeManager, _semanticModelManager);

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

        private static DataContext GetDataContext()
        {
            var builder = new DataContextBuilder();
            builder.Tables.AddRange(GetSchemaTables());
            builder.Variables.Add(new VariableSymbol("Id", typeof(int)));
            builder.PropertyProviders.DefaultValue = new ReflectionProvider();
            builder.MethodProviders.DefaultValue = new ReflectionProvider();
            return builder.GetResult();
        }

        private static IEnumerable<SchemaTableSymbol> GetSchemaTables()
        {
            return new[]
            {
                new SchemaTableSymbol("Categories", new[]
                {
                    new ColumnSymbol("CategoryID", typeof (int)),
                    new ColumnSymbol("CategoryName", typeof (string)),
                    new ColumnSymbol("Description", typeof (string)),
                    new ColumnSymbol("Picture", typeof (byte[]))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("CustomerCustomerDemo", new[]
                {
                    new ColumnSymbol("CustomerID", typeof (string)),
                    new ColumnSymbol("CustomerTypeID", typeof (string))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("CustomerDemographics", new[]
                {
                    new ColumnSymbol("CustomerTypeID", typeof (string)),
                    new ColumnSymbol("CustomerDesc", typeof (string))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("Customers", new[]
                {
                    new ColumnSymbol("CustomerID", typeof (string)),
                    new ColumnSymbol("CompanyName", typeof (string)),
                    new ColumnSymbol("ContactName", typeof (string)),
                    new ColumnSymbol("ContactTitle", typeof (string)),
                    new ColumnSymbol("Address", typeof (string)),
                    new ColumnSymbol("City", typeof (string)),
                    new ColumnSymbol("Region", typeof (string)),
                    new ColumnSymbol("PostalCode", typeof (string)),
                    new ColumnSymbol("Country", typeof (string)),
                    new ColumnSymbol("Phone", typeof (string)),
                    new ColumnSymbol("Fax", typeof (string))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("Employees", new[]
                {
                    new ColumnSymbol("EmployeeID", typeof (int)),
                    new ColumnSymbol("LastName", typeof (string)),
                    new ColumnSymbol("FirstName", typeof (string)),
                    new ColumnSymbol("Title", typeof (string)),
                    new ColumnSymbol("TitleOfCourtesy", typeof (string)),
                    new ColumnSymbol("BirthDate", typeof (DateTime)),
                    new ColumnSymbol("HireDate", typeof (DateTime)),
                    new ColumnSymbol("Address", typeof (string)),
                    new ColumnSymbol("City", typeof (string)),
                    new ColumnSymbol("Region", typeof (string)),
                    new ColumnSymbol("PostalCode", typeof (string)),
                    new ColumnSymbol("Country", typeof (string)),
                    new ColumnSymbol("HomePhone", typeof (string)),
                    new ColumnSymbol("Extension", typeof (string)),
                    new ColumnSymbol("Photo", typeof (byte[])),
                    new ColumnSymbol("Notes", typeof (string)),
                    new ColumnSymbol("ReportsTo", typeof (int)),
                    new ColumnSymbol("PhotoPath", typeof (string))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("EmployeeTerritories", new[]
                {
                    new ColumnSymbol("EmployeeID", typeof (int)),
                    new ColumnSymbol("TerritoryID", typeof (string))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("Order Details", new[]
                {
                    new ColumnSymbol("OrderID", typeof (int)),
                    new ColumnSymbol("ProductID", typeof (int)),
                    new ColumnSymbol("UnitPrice", typeof (Decimal)),
                    new ColumnSymbol("Quantity", typeof (Int16)),
                    new ColumnSymbol("Discount", typeof (Single))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("Orders", new[]
                {
                    new ColumnSymbol("OrderID", typeof (int)),
                    new ColumnSymbol("CustomerID", typeof (string)),
                    new ColumnSymbol("EmployeeID", typeof (int)),
                    new ColumnSymbol("OrderDate", typeof (DateTime)),
                    new ColumnSymbol("RequiredDate", typeof (DateTime)),
                    new ColumnSymbol("ShippedDate", typeof (DateTime)),
                    new ColumnSymbol("ShipVia", typeof (int)),
                    new ColumnSymbol("Freight", typeof (Decimal)),
                    new ColumnSymbol("ShipName", typeof (string)),
                    new ColumnSymbol("ShipAddress", typeof (string)),
                    new ColumnSymbol("ShipCity", typeof (string)),
                    new ColumnSymbol("ShipRegion", typeof (string)),
                    new ColumnSymbol("ShipPostalCode", typeof (string)),
                    new ColumnSymbol("ShipCountry", typeof (string))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("Products", new[]
                {
                    new ColumnSymbol("ProductID", typeof (int)),
                    new ColumnSymbol("ProductName", typeof (string)),
                    new ColumnSymbol("SupplierID", typeof (int)),
                    new ColumnSymbol("CategoryID", typeof (int)),
                    new ColumnSymbol("QuantityPerUnit", typeof (string)),
                    new ColumnSymbol("UnitPrice", typeof (Decimal)),
                    new ColumnSymbol("UnitsInStock", typeof (Int16)),
                    new ColumnSymbol("UnitsOnOrder", typeof (Int16)),
                    new ColumnSymbol("ReorderLevel", typeof (Int16)),
                    new ColumnSymbol("Discontinued", typeof (Boolean))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("Region", new[]
                {
                    new ColumnSymbol("RegionID", typeof (int)),
                    new ColumnSymbol("RegionDescription", typeof (string))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("Shippers", new[]
                {
                    new ColumnSymbol("ShipperID", typeof (int)),
                    new ColumnSymbol("CompanyName", typeof (string)),
                    new ColumnSymbol("Phone", typeof (string))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("Suppliers", new[]
                {
                    new ColumnSymbol("SupplierID", typeof (int)),
                    new ColumnSymbol("CompanyName", typeof (string)),
                    new ColumnSymbol("ContactName", typeof (string)),
                    new ColumnSymbol("ContactTitle", typeof (string)),
                    new ColumnSymbol("Address", typeof (string)),
                    new ColumnSymbol("City", typeof (string)),
                    new ColumnSymbol("Region", typeof (string)),
                    new ColumnSymbol("PostalCode", typeof (string)),
                    new ColumnSymbol("Country", typeof (string)),
                    new ColumnSymbol("Phone", typeof (string)),
                    new ColumnSymbol("Fax", typeof (string)),
                    new ColumnSymbol("HomePage", typeof (string))
                }, typeof (IEnumerable)),
                new SchemaTableSymbol("Territories", new[]
                {
                    new ColumnSymbol("TerritoryID", typeof (string)),
                    new ColumnSymbol("TerritoryDescription", typeof (string)),
                    new ColumnSymbol("RegionID", typeof (int))
                }, typeof (IEnumerable)),
            };
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
                    children.AddRange(child.AsToken().LeadingTrivia.Select(ToViewModel));

                children.Add(ToViewModel(child));

                if (child.IsToken)
                    children.AddRange(child.AsToken().TrailingTrivia.Select(ToViewModel));
            }

            return new NodeViewModel(node, children);
        }

        private static NodeViewModel ToViewModel(SyntaxToken token)
        {
            return new NodeViewModel(token, new List<NodeViewModel>());
        }

        private static NodeViewModel ToViewModel(SyntaxTrivia trivia)
        {
            var children = new List<NodeViewModel>();
            
            if (trivia.Structure != null)
            {
                var structureViewModel = ToViewModel(trivia.Structure);
                children.Add(structureViewModel);
            }

            return new NodeViewModel(trivia, children);
        }

        private enum NodeViewModelKind
        {
            Node,
            Token,
            Trivia
        }

        private sealed class NodeViewModel
        {
            public NodeViewModel(SyntaxToken token, IList<NodeViewModel> children)
            {
                Data = token;
                NodeType = NodeViewModelKind.Token;
                Kind = token.Kind;
                ContextualKind = token.ContextualKind;
                Span = token.Span;
                FullSpan = token.FullSpan;
                IsMissing = token.IsMissing;
                UpdateChildren(children);
            }

            public NodeViewModel(SyntaxTrivia data, IList<NodeViewModel> children)
            {
                Data = data;
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
                NodeType = NodeViewModelKind.Node;
                Kind = data.Kind;
                ContextualKind = SyntaxKind.BadToken;
                Span = data.Span;
                FullSpan = data.FullSpan;
                IsMissing = false;
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
            private readonly INQuerySyntaxTreeManager _syntaxTreeManager;
            private readonly INQuerySemanticModelManager _semanticModelManager;

            public DiagnosticsViewModel(INQuerySyntaxTreeManager syntaxTreeManager, INQuerySemanticModelManager semanticModelManager)
            {
                _syntaxTreeManager = syntaxTreeManager;
                _syntaxTreeManager.SyntaxTreeChanged += SyntaxTreeManagerOnSyntaxTreeChanged;
                _semanticModelManager = semanticModelManager;
                _semanticModelManager.SemanticModelChanged += SemanticModelManagerOnSemanticModelChanged;
                Diagnostics = new ObservableCollection<DiagnosticViewModel>();
            }

            private void SemanticModelManagerOnSemanticModelChanged(object sender, EventArgs e)
            {
                UpdateDiagnostics();
            }

            private void SyntaxTreeManagerOnSyntaxTreeChanged(object sender, EventArgs e)
            {
                UpdateDiagnostics();
            }

            private void UpdateDiagnostics()
            {
                IEnumerable<DiagnosticViewModel> diagnostics = GetDiagnostics();

                Diagnostics.Clear();
                foreach (var diagnostic in diagnostics)
                    Diagnostics.Add(diagnostic);
            }

            private IEnumerable<DiagnosticViewModel> GetDiagnostics()
            {
                var syntaxTree = _syntaxTreeManager.SyntaxTree;
                var textBuffer = syntaxTree.TextBuffer;
                var semanticModel = _semanticModelManager.SemanticModel;

                if (syntaxTree == null)
                    return Enumerable.Empty<DiagnosticViewModel>();

                var syntaxTreeDiagnostics = syntaxTree.GetDiagnostics();

                var semanticModelDiagnostics = semanticModel != null && semanticModel.Compilation.SyntaxTree == syntaxTree
                                                   ? semanticModel.GetDiagnostics()
                                                   : Enumerable.Empty<Diagnostic>();

                return from d in syntaxTreeDiagnostics.Concat(semanticModelDiagnostics)
                       orderby d.Span.Start, d.Span.End
                       select new DiagnosticViewModel(d, textBuffer);
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

        private void UpdateTree()
        {
            if (_syntaxTreeManager.SyntaxTree == null)
                return;

            var root = _syntaxTreeManager.SyntaxTree.Root;
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

        private void SyntaxTreeManagerOnSyntaxTreeChanged(object sender, EventArgs eventArgs)
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
