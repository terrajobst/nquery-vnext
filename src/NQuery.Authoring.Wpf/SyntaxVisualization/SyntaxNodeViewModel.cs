using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    internal sealed class SyntaxNodeViewModel
    {
        public SyntaxNodeViewModel(SyntaxToken data, IEnumerable<SyntaxNodeViewModel> children)
        {
            Data = data;
            Title = data.Kind.ToString();
            NodeType = SyntaxNodeViewModelKind.Token;
            Kind = data.Kind;
            ContextualKind = data.ContextualKind;
            Span = data.Span;
            FullSpan = data.FullSpan;
            IsMissing = data.IsMissing;
            UpdateChildren(children);
        }

        public SyntaxNodeViewModel(SyntaxTrivia data, bool isLeading, IEnumerable<SyntaxNodeViewModel> children)
        {
            Data = data;
            Title = string.Format("{0}:{1}", isLeading ? "L" : "T", data.Kind);
            NodeType = SyntaxNodeViewModelKind.Trivia;
            Kind = data.Kind;
            ContextualKind = SyntaxKind.BadToken;
            Span = data.Span;
            FullSpan = data.Span;
            IsMissing = false;
            UpdateChildren(children);
        }

        public SyntaxNodeViewModel(SyntaxNode data, IEnumerable<SyntaxNodeViewModel> children)
        {
            Data = data;
            Title = data.Kind.ToString();
            NodeType = SyntaxNodeViewModelKind.Node;
            Kind = data.Kind;
            ContextualKind = SyntaxKind.BadToken;
            Span = data.Span;
            FullSpan = data.FullSpan;
            IsMissing = data.IsMissing;
            UpdateChildren(children);
        }

        private void UpdateChildren(IEnumerable<SyntaxNodeViewModel> children)
        {
            Children = children.ToImmutableArray();

            foreach (var nodeViewModel in Children)
                nodeViewModel.Parent = this;
        }

        public SyntaxNodeViewModel Parent { get; private set; }

        public object Data { get; private set; }

        public string Title { get; private set; }

        public SyntaxNodeViewModelKind NodeType { get; private set; }

        public SyntaxKind Kind { get; private set; }

        public SyntaxKind ContextualKind { get; private set; }

        public TextSpan Span { get; private set; }

        public TextSpan FullSpan { get; private set; }

        public bool IsMissing { get; set; }

        public ImmutableArray<SyntaxNodeViewModel> Children { get; private set; }

        public override string ToString()
        {
            return ContextualKind == SyntaxKind.BadToken
                       ? Kind.ToString()
                       : string.Format("{0} ({1})", Kind, ContextualKind);
        }
    }
}