using System;
using System.Collections.Generic;

namespace NQueryViewer.Syntax
{
    public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _leftParentheses;
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _rightParentheses;

        public ParenthesizedExpressionSyntax(SyntaxToken leftParentheses, ExpressionSyntax expression, SyntaxToken rightParentheses)
        {
            _leftParentheses = leftParentheses;
            _expression = expression;
            _rightParentheses = rightParentheses;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> GetChildren()
        {
            yield return _leftParentheses;
            yield return _expression;
            yield return _rightParentheses;
        }

        public SyntaxToken LeftParentheses
        {
            get { return _leftParentheses; }
        }

        public ExpressionSyntax Expression
        {
            get { return _expression; }
        }

        public SyntaxToken RightParentheses
        {
            get { return _rightParentheses; }
        }
    }
}