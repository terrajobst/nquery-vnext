using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class CountAllExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _name;
        private readonly SyntaxToken _leftParenthesis;
        private readonly SyntaxToken _asteriskToken;
        private readonly SyntaxToken _rightParenthesis;

        internal CountAllExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, SyntaxToken leftParenthesis, SyntaxToken asteriskToken, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            _name = identifier;
            _leftParenthesis = leftParenthesis;
            _asteriskToken = asteriskToken;
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CountAllExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _name;
            yield return _leftParenthesis;
            yield return _asteriskToken;
            yield return _rightParenthesis;
        }

        public SyntaxToken Name
        {
            get { return _name; }
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