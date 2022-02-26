namespace NQuery.Syntax
{
    public sealed class CommonTableExpressionQuerySyntax : QuerySyntax
    {
        internal CommonTableExpressionQuerySyntax(SyntaxTree syntaxTree, SyntaxToken withKeyword, SeparatedSyntaxList<CommonTableExpressionSyntax> commonTableExpressions, QuerySyntax query)
            : base(syntaxTree)
        {
            WithKeyword = withKeyword;
            Query = query;
            CommonTableExpressions = commonTableExpressions;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CommonTableExpressionQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return WithKeyword;

            foreach (var nodeOrToken in CommonTableExpressions.GetWithSeparators())
                yield return nodeOrToken;

            yield return Query;
        }

        public SyntaxToken WithKeyword { get; }

        public SeparatedSyntaxList<CommonTableExpressionSyntax> CommonTableExpressions { get; }

        public QuerySyntax Query { get; }
    }
}