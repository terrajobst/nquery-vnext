namespace NQuery.Syntax
{
    public sealed class AllAnySubselectSyntax : SubselectExpressionSyntax
    {
        internal AllAnySubselectSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, SyntaxToken keyword, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            Left = left;
            OperatorToken = operatorToken;
            Keyword = keyword;
            LeftParenthesis = leftParenthesis;
            Query = query;
            RightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            // TODO: May be we should have different values for ALL, ANY, and SOME?
            get { return SyntaxKind.AllAnySubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            yield return OperatorToken;
            yield return Keyword;
            yield return LeftParenthesis;
            yield return Query;
            yield return RightParenthesis;
        }

        public ExpressionSyntax Left { get; }

        public SyntaxToken OperatorToken { get; }

        public SyntaxToken Keyword { get; }

        public SyntaxToken LeftParenthesis { get; }

        public QuerySyntax Query { get; }

        public SyntaxToken RightParenthesis { get; }
    }
}