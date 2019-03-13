#nullable enable

using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        internal UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken operatorToken, ExpressionSyntax expression)
            : base(syntaxTree)
        {
            OperatorToken = operatorToken;
            Expression = expression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxFacts.GetUnaryOperatorExpression(OperatorToken.Kind); }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return OperatorToken;
            yield return Expression;
        }

        public SyntaxToken OperatorToken { get; }

        public ExpressionSyntax Expression { get; }
    }
}