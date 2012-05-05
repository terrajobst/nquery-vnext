using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQueryViewer.Syntax
{
    public sealed class ArgumentListSyntax : SyntaxNode
    {
        private readonly SyntaxToken _leftParenthesis;
        private readonly IList<ArgumentSyntax> _arguments;
        private readonly SyntaxToken _rightParenthesis;

        public ArgumentListSyntax(SyntaxToken leftParenthesis, IList<ArgumentSyntax> arguments, SyntaxToken rightParenthesis)
        {
            _leftParenthesis = leftParenthesis;
            _arguments = new ReadOnlyCollection<ArgumentSyntax>(arguments);
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ArgumentList; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
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