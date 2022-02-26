namespace NQuery.Syntax
{
    public sealed class ExpressionSelectColumnSyntax : SelectColumnSyntax
    {
        internal ExpressionSelectColumnSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, AliasSyntax alias)
            : base(syntaxTree)
        {
            Expression = expression;
            Alias = alias;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.ExpressionSelectColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Expression;
            if (Alias != null)
                yield return Alias;
        }

        public ExpressionSyntax Expression { get; }

        public AliasSyntax Alias { get; }
    }
}