using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        internal LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken token, object value)
            : base(syntaxTree)
        {
            Token = token;
            Value = value;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.LiteralExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Token;
        }

        public SyntaxToken Token { get; }

        public object Value { get; }
    }
}