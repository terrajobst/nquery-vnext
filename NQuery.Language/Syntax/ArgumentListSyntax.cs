using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language
{
    public sealed class ArgumentListSyntax : SyntaxNode
    {
        private readonly SyntaxToken _leftParenthesis;
        private readonly IList<ArgumentSyntax> _arguments;
        private readonly SyntaxToken _rightParenthesis;

        public ArgumentListSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, IList<ArgumentSyntax> arguments, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            _leftParenthesis = leftParenthesis.WithParent(this);
            _arguments = new ReadOnlyCollection<ArgumentSyntax>(arguments);
            _rightParenthesis = rightParenthesis.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ArgumentList; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _leftParenthesis;

            foreach (var argument in _arguments)
                yield return argument;

            yield return _rightParenthesis;
        }

        public SyntaxToken LeftParenthesis
        {
            get { return _leftParenthesis; }
        }

        public IList<ArgumentSyntax> Arguments
        {
            get { return _arguments; }
        }

        public SyntaxToken RightParenthesis
        {
            get { return _rightParenthesis; }
        }
    }
}