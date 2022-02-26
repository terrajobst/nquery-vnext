namespace NQuery.Syntax
{
    public sealed class NameExpressionSyntax : ExpressionSyntax
    {
        internal NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken name)
            : base(syntaxTree)
        {
            Name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NameExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Name;
        }

        public SyntaxToken Name { get; }
    }
}