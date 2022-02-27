namespace NQuery.Syntax
{
    public sealed class UnionQuerySyntax : QuerySyntax
    {
        internal UnionQuerySyntax(SyntaxTree syntaxTree, QuerySyntax leftQuery, SyntaxToken unionKeyword, SyntaxToken allKeyword, QuerySyntax rightQuery)
            : base(syntaxTree)
        {
            LeftQuery = leftQuery;
            UnionKeyword = unionKeyword;
            AllKeyword = allKeyword;
            RightQuery = rightQuery;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.UnionQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return LeftQuery;
            yield return UnionKeyword;
            if (AllKeyword is not null)
                yield return AllKeyword;
            yield return RightQuery;
        }

        public QuerySyntax LeftQuery { get; }

        public SyntaxToken UnionKeyword { get; }

        public SyntaxToken AllKeyword { get; }

        public QuerySyntax RightQuery { get; }
    }
}