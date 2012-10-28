using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Authoring.Wpf
{
    internal sealed class SyntaxTreeViewModel
    {
        private readonly SyntaxTree _model;
        private readonly ReadOnlyCollection<SyntaxNodeViewModel> _root;

        public SyntaxTreeViewModel(SyntaxTree model, SyntaxNodeViewModel root)
        {
            _model = model;
            _root = new ReadOnlyCollection<SyntaxNodeViewModel>(new[] { root });
        }

        public SyntaxTree Model
        {
            get { return _model; }
        }

        public ReadOnlyCollection<SyntaxNodeViewModel> Root
        {
            get { return _root; }
        }

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