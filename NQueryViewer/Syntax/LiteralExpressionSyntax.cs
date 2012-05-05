using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _token;
        private readonly object _value;

        public LiteralExpressionSyntax(SyntaxToken token, object value)
        {
            _token = token;
            _value = value;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.LiteralExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
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