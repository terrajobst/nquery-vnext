using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class IsNullExpressionSyntax : ExpressionSyntax
    {
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _isKeyword;
        private readonly SyntaxToken? _notKeyword;
        private readonly SyntaxToken _nullKeyword;

        public IsNullExpressionSyntax(ExpressionSyntax expression, SyntaxToken isKeyword, SyntaxToken? notKeyword, SyntaxToken nullKeyword)
        {
            _expression = expression;
            _isKeyword = isKeyword;
            _notKeyword = notKeyword;
            _nullKeyword = nullKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.IsNullExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _expression;
            yield return _isKeyword;
            if (_notKeyword != null)
                yield return _notKeyword.Value;
            yield return _nullKeyword;
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken IsKeyword
        {
            get { return _isKeyword; }
        }

        public SyntaxToken? NotKeyword
        {
            get { return _notKeyword; }
        }

        public SyntaxToken NullKeyword
        {
            get { return _nullKeyword; }
        }
    }
}