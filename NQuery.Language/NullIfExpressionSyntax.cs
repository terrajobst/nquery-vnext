using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class NullIfExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _nullIfKeyword;
        private readonly SyntaxToken _leftParenthesesToken;
        private readonly ExpressionSyntax _leftExpression;
        private readonly SyntaxToken _commaToken;
        private readonly ExpressionSyntax _rightExpression;
        private readonly SyntaxToken _rightParenthesesToken;

        public NullIfExpressionSyntax(SyntaxToken nullIfKeyword, SyntaxToken leftParenthesesToken, ExpressionSyntax leftExpression, SyntaxToken commaToken, ExpressionSyntax rightExpression, SyntaxToken rightParenthesesToken)
        {
            _nullIfKeyword = nullIfKeyword;
            _leftParenthesesToken = leftParenthesesToken;
            _leftExpression = leftExpression;
            _commaToken = commaToken;
            _rightExpression = rightExpression;
            _rightParenthesesToken = rightParenthesesToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NullIfExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _nullIfKeyword;
            yield return _leftParenthesesToken;
            yield return _leftExpression;
            yield return _commaToken;
            yield return _rightExpression;
            yield return _rightParenthesesToken;
        }

        public SyntaxToken NullIfKeyword
        {
            get { return _nullIfKeyword; }
        }

        public SyntaxToken LeftParenthesesToken
        {
            get { return _leftParenthesesToken; }
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

        public SyntaxToken RightParenthesesToken
        {
            get { return _rightParenthesesToken; }
        }
    }
}