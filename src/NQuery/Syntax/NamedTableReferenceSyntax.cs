namespace NQuery.Syntax
{
    public sealed class NamedTableReferenceSyntax : TableReferenceSyntax
    {
        internal NamedTableReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken tableName, AliasSyntax alias)
            : base(syntaxTree)
        {
            TableName = tableName;
            Alias = alias;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NamedTableReference; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return TableName;
            if (Alias != null)
                yield return Alias;
        }

        public SyntaxToken TableName { get; }

        public AliasSyntax Alias { get; }
    }
}