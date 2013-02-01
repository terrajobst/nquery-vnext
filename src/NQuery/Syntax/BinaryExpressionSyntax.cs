using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _left;
        private readonly SyntaxToken _operatorToken;
        private readonly ExpressionSyntax _right;

        public BinaryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
            : base(syntaxTree)
        {
            _left = left;
            _operatorToken = operatorToken;
            _right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxFacts.GetBinaryOperatorExpression(_operatorToken.Kind); }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
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