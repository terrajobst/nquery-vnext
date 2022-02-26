namespace NQuery.Syntax
{
    public sealed class FromClauseSyntax : SyntaxNode
    {
        internal FromClauseSyntax(SyntaxTree syntaxTree, SyntaxToken fromKeyword, SeparatedSyntaxList<TableReferenceSyntax> tableReferences)
            : base(syntaxTree)
        {
            FromKeyword = fromKeyword;
            TableReferences = tableReferences;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.FromClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return FromKeyword;
            foreach (var nodeOrToken in TableReferences.GetWithSeparators())
                yield return nodeOrToken;
        }

        public SyntaxToken FromKeyword { get; }

        public SeparatedSyntaxList<TableReferenceSyntax> TableReferences { get; }
    }
}