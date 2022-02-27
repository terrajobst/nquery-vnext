namespace NQuery.Syntax
{
    public sealed class OrderByColumnSyntax : SyntaxNode
    {
        internal OrderByColumnSyntax(SyntaxTree syntaxTree, ExpressionSyntax columnSelector, SyntaxToken modifier)
            : base(syntaxTree)
        {
            ColumnSelector = columnSelector;
            Modifier = modifier;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.OrderByColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return ColumnSelector;
            if (Modifier is not null)
                yield return Modifier;
        }

        public ExpressionSyntax ColumnSelector { get; }

        public SyntaxToken Modifier { get; }
    }
}