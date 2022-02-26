using System.Collections.Immutable;

namespace NQuery.Authoring.Wpf
{
    internal sealed class SyntaxTreeViewModel
    {
        public SyntaxTreeViewModel(SyntaxTree model, SyntaxNodeViewModel root)
        {
            Model = model;
            Root = ImmutableArray.Create(root);
        }

        public SyntaxTree Model { get; }

        public ImmutableArray<SyntaxNodeViewModel> Root { get; }

        public static SyntaxTreeViewModel Create(SyntaxTree syntaxTree)
        {
            var root = ToViewModel(syntaxTree.Root);
            return new SyntaxTreeViewModel(syntaxTree, root);
        }

        private static SyntaxNodeViewModel ToViewModel(SyntaxNodeOrToken nodeOrToken)
        {
            return nodeOrToken.IsNode
                       ? ToViewModel(nodeOrToken.AsNode())
                       : ToViewModel(nodeOrToken.AsToken());
        }

        private static SyntaxNodeViewModel ToViewModel(SyntaxNode node)
        {
            var children = new List<SyntaxNodeViewModel>();

            foreach (var child in node.ChildNodesAndTokens())
            {
                if (child.IsToken)
                    children.AddRange(child.AsToken().LeadingTrivia.Select(t => ToViewModel(t, true)));

                children.Add(ToViewModel(child));

                if (child.IsToken)
                    children.AddRange(child.AsToken().TrailingTrivia.Select(t => ToViewModel(t, false)));
            }

            return new SyntaxNodeViewModel(node, children);
        }

        private static SyntaxNodeViewModel ToViewModel(SyntaxToken token)
        {
            return new SyntaxNodeViewModel(token, new List<SyntaxNodeViewModel>());
        }

        private static SyntaxNodeViewModel ToViewModel(SyntaxTrivia trivia, bool isLeading)
        {
            var children = new List<SyntaxNodeViewModel>();

            if (trivia.Structure != null)
            {
                var structureViewModel = ToViewModel(trivia.Structure);
                children.Add(structureViewModel);
            }

            return new SyntaxNodeViewModel(trivia, isLeading, children);
        }
    }
}