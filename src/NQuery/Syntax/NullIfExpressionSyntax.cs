using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class NullIfExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _nullIfKeyword;
        private readonly SyntaxToken _leftParenthesisToken;
        private readonly ExpressionSyntax _leftExpression;
        private readonly SyntaxToken _commaToken;
        private readonly ExpressionSyntax _rightExpression;
        private readonly SyntaxToken _rightParenthesisToken;

        internal NullIfExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken nullIfKeyword, SyntaxToken leftParenthesisToken, ExpressionSyntax leftExpression, SyntaxToken commaToken, ExpressionSyntax rightExpression, SyntaxToken rightParenthesisToken)
            : base(syntaxTree)
        {
            _nullIfKeyword = nullIfKeyword;
            _leftParenthesisToken = leftParenthesisToken;
            _leftExpression = leftExpression;
            _commaToken = commaToken;
            _rightExpression = rightExpression;
            _rightParenthesisToken = rightParenthesisToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NullIfExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _nullIfKeyword;
            yield return _leftParenthesisToken;
            yield return _leftExpression;
            yield return _commaToken;
            yield return _rightExpression;
            yield return _rightParenthesisToken;
        }

        public SyntaxToken NullIfKeyword
        {
            get { return _nullIfKeyword; }
        }

        public SyntaxToken LeftParenthesisToken
        {
            get { return _leftParenthesisToken; }
        }

        public ExpressionSyntax LeftExpression
        {
            get { return _leftExpression; }
        }

        public SyntaxToken CommaToken
        {
            get { return _commaToken; }
        }

        public ExpressionSyntax RightExpression
        {
            get { return _rightExpression; }
        }

        public SyntaxToken RightParenthesisToken
        {
            get { return _rightParenthesisToken; }
        }
    }
}