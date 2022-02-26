namespace NQuery.Syntax
{
    public sealed class ParenthesizedQuerySyntax : QuerySyntax
    {
        internal ParenthesizedQuerySyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis)
            : base(syntaxTree)
        {
            LeftParenthesis = leftParenthesis;
            Query = query;
            RightParenthesis = rightParenthesis;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ParenthesizedQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return LeftParenthesis;
            yield return Query;
            yield return RightParenthesis;
        }

        public SyntaxToken LeftParenthesis { get; }

        public QuerySyntax Query { get; }

        public SyntaxToken RightParenthesis { get; }
    }
}