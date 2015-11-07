using System;
using System.Collections.Generic;

namespace NQuery.Syntax
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _token;
        private readonly object _value;

        internal LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken token, object value)
            : base(syntaxTree)
        {
            _token = token;
            _value = value;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.LiteralExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return _token;
        }

        public SyntaxToken Token
        {
            get { return _token; }
        }

        public object Value
        {
            get { return _value; }
        }
    }
}