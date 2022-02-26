namespace NQuery.Syntax
{
    public sealed class ExistsSubselectSyntax : SubselectExpressionSyntax
    {
        internal ExistsSubselectSyntax(SyntaxTree syntaxTree, SyntaxToken existsKeyword, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            ExistsKeyword = existsKeyword;
            LeftParenthesis = leftParenthesis;
            Query = query;
            RightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExistsSubselect; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return ExistsKeyword;
            yield return LeftParenthesis;
            yield return Query;
            yield return RightParenthesis;
        }

        public SyntaxToken ExistsKeyword { get; }

        public SyntaxToken LeftParenthesis { get; }

        public QuerySyntax Query { get; }

        public SyntaxToken RightParenthesis { get; }
    }
}