namespace NQuery.Syntax
{
    public sealed class DerivedTableReferenceSyntax : TableReferenceSyntax
    {
        internal DerivedTableReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesis, QuerySyntax query, SyntaxToken rightParenthesis, SyntaxToken asKeyword, SyntaxToken name)
            : base(syntaxTree)
        {
            LeftParenthesis = leftParenthesis;
            Query = query;
            RightParenthesis = rightParenthesis;
            AsKeyword = asKeyword;
            Name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.DerivedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return LeftParenthesis;
            yield return Query;
            yield return RightParenthesis;
            if (AsKeyword is not null)
                yield return AsKeyword;
            yield return Name;
        }

        public SyntaxToken LeftParenthesis { get; }

        public QuerySyntax Query { get; }

        public SyntaxToken RightParenthesis { get; }

        public SyntaxToken AsKeyword { get; }

        public SyntaxToken Name { get; }
    }
}