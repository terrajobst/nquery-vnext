using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _leftParenthesis;
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _rightParenthesis;

        public ParenthesizedExpressionSyntax(SyntaxToken leftParenthesis, ExpressionSyntax expression, SyntaxToken rightParenthesis)
        {
            _leftParenthesis = leftParenthesis;
            _expression = expression;
            _rightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParenthesis;
            yield return _expression;
            yield return _rightParenthesis;
        }

        public SyntaxToken LeftParenthesis
        {
            get { return _leftParenthesis; }
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken RightParenthesis
        {
            get { return _rightParenthesis; }
        }
    }
}