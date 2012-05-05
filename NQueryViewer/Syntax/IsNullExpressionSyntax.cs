using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class IsNullExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _isToken;
        private readonly SyntaxToken? _notToken;
        private readonly SyntaxToken _nullToken;

        public IsNullExpressionSyntax(ExpressionSyntax expression, SyntaxToken isToken, SyntaxToken? notToken, SyntaxToken nullToken)
        {
            _expression = expression;
            _isToken = isToken;
            _notToken = notToken;
            _nullToken = nullToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.IsNullExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _expression;
            yield return _isToken;
            if (_notToken != null)
                yield return _notToken.Value;
            yield return _nullToken;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken IsToken
        {
            get { return _isToken; }
        }

        public SyntaxToken? NotToken
        {
            get { return _notToken; }
        }

        public SyntaxToken NullToken
        {
            get { return _nullToken; }
        }
    }
}