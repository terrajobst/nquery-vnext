using System;
using System.Collections.Generic;

namespace NQuery.Language
{
    public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        private readonly SyntaxToken _leftParenthesis;
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxToken _rightParenthesis;

        public ParenthesizedExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, ExpressionSyntax expression, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            _leftParenthesis = leftParenthesis.WithParent(this);
            _expression = expression;
            _rightParenthesis = rightParenthesis.WithParent(this);
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
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