namespace NQuery.Syntax
{
    public sealed class CaseLabelSyntax : SyntaxNode
    {
        internal CaseLabelSyntax(SyntaxTree syntaxTree, SyntaxToken whenKeyword, ExpressionSyntax whenExpression, SyntaxToken thenKeyword, ExpressionSyntax thenExpression)
            : base(syntaxTree)
        {
            WhenKeyword = whenKeyword;
            WhenExpression = whenExpression;
            ThenKeyword = thenKeyword;
            ThenExpression = thenExpression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CaseLabel; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return WhenKeyword;
            yield return WhenExpression;
            yield return ThenKeyword;
            yield return ThenExpression;
        }

        public SyntaxToken WhenKeyword { get; }

        public ExpressionSyntax WhenExpression { get; }

        public SyntaxToken ThenKeyword { get; }

        public ExpressionSyntax ThenExpression { get; }
    }
}