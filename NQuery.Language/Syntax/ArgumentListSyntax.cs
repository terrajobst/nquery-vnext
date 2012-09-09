using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ArgumentListSyntax : SyntaxNode
    {
        private readonly SyntaxToken _leftParenthesis;
        private readonly SeparatedSyntaxList<ExpressionSyntax> _arguments;
        private readonly SyntaxToken _rightParenthesis;

        public ArgumentListSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            _leftParenthesis = leftParenthesis.WithParent(this);
            _arguments = arguments.WithParent(this);
            _rightParenthesis = rightParenthesis.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ArgumentList; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _leftParenthesis;

            foreach (var nodeOrToken in _arguments.GetWithSeparators())
                yield return nodeOrToken;

            yield return _rightParenthesis;
        }

        public SyntaxToken LeftParenthesis
        {
            get { return _leftParenthesis; }
        }

        public SeparatedSyntaxList<ExpressionSyntax> Arguments
        {
            get { return _arguments; }
        }

        public SyntaxToken RightParenthesis
        {
            get { return _rightParenthesis; }
        }
    }
}