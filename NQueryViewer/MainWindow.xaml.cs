using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using NQueryViewer.Syntax;

namespace NQueryViewer
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            UpdateViewModel();
        }

        private static IEnumerable<NodeViewModel> ToViewModel(string source)
        {
            //return Lex(source);
            //return ParseExpression(source);
            return ParseQuery(source);
        }

        private static IEnumerable<NodeViewModel> Lex(string source)
        {
            var lexer = new Lexer(source);
            var tokens = new List<SyntaxToken>();

            SyntaxToken token;
            do
            {
                token = lexer.Lex();
                tokens.Add(token);
            } while (token.Kind != SyntaxKind.EndOfFileToken);

            return ToViewModel(tokens);
        }

        private static IEnumerable<NodeViewModel> ParseExpression(string source)
        {
            var parser = new Parser(source);
            var expression = parser.ParseExpression();
            return new[] {ToViewModel(expression)};
        }

        private static IEnumerable<NodeViewModel> ParseQuery(string source)
        {
            var parser = new Parser(source);
            var query = parser.ParseQueryWithOptionalCte();
            return new[] { ToViewModel(query) };
        }

        private static IEnumerable<NodeViewModel> ToViewModel(IEnumerable<SyntaxToken> root)
        {
            return root.Select(ToViewModel).ToList();
        }

        private static NodeViewModel ToViewModel(SyntaxNodeOrToken nodeOrToken)
        {
            return nodeOrToken.IsNode
                       ? ToViewModel(nodeOrToken.AsNode)
                       : ToViewModel(nodeOrToken.AsToken);
        }

        private static NodeViewModel ToViewModel(SyntaxNode node)
        {
            var children = new List<NodeViewModel>();

            foreach (var child in node.GetChildren())
            {
                if (child.IsToken)
                    children.AddRange(child.AsToken.LeadingTrivia.Select(ToViewModel));

                children.Add(ToViewModel(child));

                if (child.IsToken)
                    children.AddRange(child.AsToken.TrailingTrivia.Select(ToViewModel));
            }

            return new NodeViewModel(node, children);
        }

        private static NodeViewModel ToViewModel(SyntaxToken token)
        {
            return new NodeViewModel(token, new List<NodeViewModel>());
        }

        private static NodeViewModel ToViewModel(SyntaxTrivia trivia)
        {
            return new NodeViewModel(trivia, new List<NodeViewModel>());
        }

        private sealed class NodeViewModel
        {
            public NodeViewModel(SyntaxToken token, IList<NodeViewModel> children)
            {
                Data = token;
                NodeType = "Token";
                Kind = token.Kind;
                ContextualKind = token.ContextualKind;
                Span = token.Span;
                FullSpan = token.FullSpan;
                IsMissing = token.IsMissing;
                Children = new ReadOnlyCollection<NodeViewModel>(children);
            }

            public NodeViewModel(SyntaxTrivia data, IList<NodeViewModel> children)
            {
                Data = data;
                NodeType = "Trivia";
                Kind = data.Kind;
                ContextualKind = SyntaxKind.BadToken;
                Span = data.Span;
                FullSpan = data.Span;
                IsMissing = false;
                Children = new ReadOnlyCollection<NodeViewModel>(children);
            }

            public NodeViewModel(SyntaxNode data, List<NodeViewModel> children)
            {
                Data = data;
                NodeType = "Node";
                Kind = data.Kind;
                ContextualKind = SyntaxKind.BadToken;
                Span = data.Span;
                FullSpan = data.FullSpan;
                IsMissing = false;
                Children = new ReadOnlyCollection<NodeViewModel>(children);
            }

            public object Data { get; private set; }

            public string NodeType { get; private set; }

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

        private void UpdateViewModel()
        {
            _treeView.ItemsSource = ToViewModel(_textBox.Text);
        }

        private void UpdateSelectedText()
        {
            var viewModel = _treeView.SelectedItem as NodeViewModel;
            if (viewModel == null)
                return;

            var span = viewModel.FullSpan;

            _textBox.Select(span.Start, span.Length);
        }

        private void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateViewModel();
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            UpdateSelectedText();
        }

    }
}
