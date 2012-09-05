using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class CountAllExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _identifier;
        private readonly SyntaxToken _leftParenthesis;
        private readonly SyntaxToken _asteriskToken;
        private readonly SyntaxToken _rightParenthesis;

        public CountAllExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, SyntaxToken leftParenthesis, SyntaxToken asteriskToken, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            _identifier = identifier.WithParent(this);
            _leftParenthesis = leftParenthesis.WithParent(this);
            _asteriskToken = asteriskToken.WithParent(this);
            _rightParenthesis = rightParenthesis.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CountAllExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _identifier;
            yield return _leftParenthesis;
            yield return _asteriskToken;
            yield return _rightParenthesis;
        }

        public SyntaxToken Identifier
        {
            get { return _identifier; }
        }

        public SyntaxToken LeftParenthesis
        {
            get { return _leftParenthesis; }
        }

        public SyntaxToken AsteriskToken
        {
            get { return _asteriskToken; }
        }

        public SyntaxToken RightParenthesis
        {
            get { return _rightParenthesis; }
        }
    }
}