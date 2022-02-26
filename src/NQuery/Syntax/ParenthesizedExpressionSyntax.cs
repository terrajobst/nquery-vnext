namespace NQuery.Syntax
{
    public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        internal ParenthesizedExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, ExpressionSyntax expression, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            LeftParenthesis = leftParenthesis;
            Expression = expression;
            RightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return LeftParenthesis;
            yield return Expression;
            yield return RightParenthesis;
        }

        public SyntaxToken LeftParenthesis { get; }

        public ExpressionSyntax Expression { get; }

        public SyntaxToken RightParenthesis { get; }
    }
}