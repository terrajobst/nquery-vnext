namespace NQuery.Syntax
{
    // TODO: Do we need this element at all?
    public sealed class GroupByColumnSyntax : SyntaxNode
    {
        internal GroupByColumnSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression)
            : base(syntaxTree)
        {
            Expression = expression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.GroupByColumn; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Expression;
        }

        public ExpressionSyntax Expression { get; }
    }
}