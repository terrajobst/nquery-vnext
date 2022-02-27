namespace NQuery.Syntax
{
    public sealed class SelectQuerySyntax : QuerySyntax
    {
        internal SelectQuerySyntax(SyntaxTree syntaxTree, SelectClauseSyntax selectClause, FromClauseSyntax fromClause, WhereClauseSyntax whereClause, GroupByClauseSyntax groupByClause, HavingClauseSyntax havingClause)
            : base(syntaxTree)
        {
            SelectClause = selectClause;
            FromClause = fromClause;
            WhereClause = whereClause;
            GroupByClause = groupByClause;
            HavingClause = havingClause;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SelectQuery; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return SelectClause;
            if (FromClause is not null)
                yield return FromClause;
            if (WhereClause is not null)
                yield return WhereClause;
            if (GroupByClause is not null)
                yield return GroupByClause;
            if (HavingClause is not null)
                yield return HavingClause;
        }

        public SelectClauseSyntax SelectClause { get; }

        public FromClauseSyntax FromClause { get; }

        public WhereClauseSyntax WhereClause { get; }

        public GroupByClauseSyntax GroupByClause { get; }

        public HavingClauseSyntax HavingClause { get; }
    }
}