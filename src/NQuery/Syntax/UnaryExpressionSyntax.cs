using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _operatorToken;
        private readonly ExpressionSyntax _expression;

        internal UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken operatorToken, ExpressionSyntax expression)
            : base(syntaxTree)
        {
            _operatorToken = operatorToken;
            _expression = expression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxFacts.GetUnaryOperatorExpression(_operatorToken.Kind); }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _operatorToken;
            yield return _expression;
        }

        public SyntaxToken OperatorToken
        {
            get { return _operatorToken; }
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }
    }
}