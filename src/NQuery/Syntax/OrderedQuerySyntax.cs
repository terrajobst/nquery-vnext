namespace NQuery.Syntax
{
    public sealed class OrderedQuerySyntax : QuerySyntax
    {
        internal OrderedQuerySyntax(SyntaxTree syntaxTree, QuerySyntax query, SyntaxToken orderKeyword, SyntaxToken byKeyword, SeparatedSyntaxList<OrderByColumnSyntax> columns)
            : base(syntaxTree)
        {
            Query = query;
            OrderKeyword = orderKeyword;
            ByKeyword = byKeyword;
            Columns = columns;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OrderedQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Query;
            yield return OrderKeyword;
            yield return ByKeyword;
            foreach (var nodeOrToken in Columns.GetWithSeparators())
                yield return nodeOrToken;
        }

        public QuerySyntax Query { get; }

        public SyntaxToken OrderKeyword { get; }

        public SyntaxToken ByKeyword { get; }

        public SeparatedSyntaxList<OrderByColumnSyntax> Columns { get; }
    }
}