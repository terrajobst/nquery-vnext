using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken _operatorToken;
        private readonly ExpressionSyntax _right;

        public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
        {
            _left = left;
            _operatorToken = operatorToken;
            _right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxFacts.GetBinaryOperatorExpression(_operatorToken.Kind); }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _left;
            yield return _operatorToken;
            yield return _right;
        }

        public ExpressionSyntax Left
        {
            get { return _left; }
        }

        public SyntaxToken OperatorToken
        {
            get { return _operatorToken; }
        }

        public ExpressionSyntax Right
        {
            get { return _right; }
        }
    }
}