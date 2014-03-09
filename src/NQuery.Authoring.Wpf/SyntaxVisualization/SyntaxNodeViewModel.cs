using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Text;

namespace NQuery.Authoring.Wpf
{
    internal sealed class SyntaxNodeViewModel
    {
        public SyntaxNodeViewModel(SyntaxToken data, IList<SyntaxNodeViewModel> children)
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

        public SyntaxNodeViewModel(SyntaxTrivia data, bool isLeading, IList<SyntaxNodeViewModel> children)
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

        public SyntaxNodeViewModel(SyntaxNode data, IList<SyntaxNodeViewModel> children)
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

        private void UpdateChildren(IList<SyntaxNodeViewModel> children)
        {
            Children = new ReadOnlyCollection<SyntaxNodeViewModel>(children);

            foreach (var nodeViewModel in children)
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

        public ReadOnlyCollection<SyntaxNodeViewModel> Children { get; private set; }

        public override string ToString()
        {
            return ContextualKind == SyntaxKind.BadToken
                       ? Kind.ToString()
                       : string.Format("{0} ({1})", Kind, ContextualKind);
        }
    }
}