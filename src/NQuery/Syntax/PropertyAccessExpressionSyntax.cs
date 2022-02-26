namespace NQuery.Syntax
{
    public sealed class PropertyAccessExpressionSyntax : ExpressionSyntax
    {
        internal PropertyAccessExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax target, SyntaxToken dot, SyntaxToken name)
            : base(syntaxTree)
        {
            Target = target;
            Dot = dot;
            Name = name;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.PropertyAccessExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Target;
            yield return Dot;
            yield return Name;
        }

        public ExpressionSyntax Target { get; }

        public SyntaxToken Dot { get; }

        public SyntaxToken Name { get; }
    }
}