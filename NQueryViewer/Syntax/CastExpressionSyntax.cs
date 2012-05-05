using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class CastExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _castKeyword;
        private readonly SyntaxToken _leftParenthesesToken;
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _asKeyword;
        private readonly TypeReferenceSyntax _typeReference;
        private readonly SyntaxToken _rightParenthesesToken;

        public CastExpressionSyntax(SyntaxToken castKeyword, SyntaxToken leftParenthesesToken, ExpressionSyntax expression, SyntaxToken asKeyword, TypeReferenceSyntax typeReference, SyntaxToken rightParenthesesToken)
        {
            _castKeyword = castKeyword;
            _leftParenthesesToken = leftParenthesesToken;
            _expression = expression;
            _asKeyword = asKeyword;
            _typeReference = typeReference;
            _rightParenthesesToken = rightParenthesesToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CastExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _castKeyword;
            yield return _leftParenthesesToken;
            yield return _expression;
            yield return _asKeyword;
            yield return _typeReference;
            yield return _rightParenthesesToken;
        }
    }
}