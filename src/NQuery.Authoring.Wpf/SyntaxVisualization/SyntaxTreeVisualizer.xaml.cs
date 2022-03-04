using System.Windows;

using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    public sealed partial class SyntaxTreeVisualizer
    {
        public static readonly DependencyProperty SyntaxTreeProperty = DependencyProperty.Register("SyntaxTree", typeof(SyntaxTree), typeof(SyntaxTreeVisualizer), new PropertyMetadata(null, PropertyChangedCallback));

        public SyntaxTreeVisualizer()
        {
            InitializeComponent();
        }

        private static void PropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var sender = o as SyntaxTreeVisualizer;
            if (sender is not null)
                sender.UpdateViewModel();
        }

        private static SyntaxNodeViewModel FindViewModelNode(IEnumerable<SyntaxNodeViewModel> roots, int position)
        {
            var nonTrivia = from r in roots
                            where r.NodeType != SyntaxNodeViewModelKind.Trivia
                            select r;

            var skippedTokens = from r in roots
                                where r.Kind == SyntaxKind.SkippedTokensTrivia
                                from c in r.Children
                                select c;

            var children = nonTrivia.Concat(skippedTokens);

            foreach (var nodeViewModel in children)
            {
                if (nodeViewModel.Span.ContainsOrTouches(position))
                {
                    return nodeViewModel.Children.Any()
                               ? FindViewModelNode(nodeViewModel.Children, position)
                               : nodeViewModel;
                }
            }

            return null;
        }

        public SyntaxTree SyntaxTree
        {
            get { return (SyntaxTree)GetValue(SyntaxTreeProperty); }
            set { SetValue(SyntaxTreeProperty, value); }
        }

        private SyntaxTreeViewModel SyntaxTreeViewModel
        {
            get { return DataContext as SyntaxTreeViewModel; }
        }

        public TextSpan? SelectedSpan
        {
            get
            {
                var viewModel = TreeView.SelectedItem as SyntaxNodeViewModel;
                return viewModel is null ? (TextSpan?)null : viewModel.Span;
            }
        }

        public TextSpan? SelectedFullSpan
        {
            get
            {
                var viewModel = TreeView.SelectedItem as SyntaxNodeViewModel;
                return viewModel is null ? (TextSpan?)null : viewModel.FullSpan;
            }
        }

        private void UpdateViewModel()
        {
            DataContext = SyntaxTree is null
                              ? null
                              : SyntaxTreeViewModel.Create(SyntaxTree);
        }

        public void SelectNode(int position)
        {
            var model = SyntaxTreeViewModel;
            if (model is null)
                return;

            var node = FindViewModelNode(model.Root, position);
            if (node is null)
                return;

            TreeView.SelectNode(node, n => n.Parent, true);
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            OnSelectedNodeChanged();
        }

        private void OnSelectedNodeChanged()
        {
            var handler = SelectedNodeChanged;
            if (handler is not null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler SelectedNodeChanged;
    }
}