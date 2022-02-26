namespace NQuery.Syntax
{
    public sealed class HavingClauseSyntax : SyntaxNode
    {
        internal HavingClauseSyntax(SyntaxTree syntaxTree, SyntaxToken havingKeyword, ExpressionSyntax predicate)
            : base(syntaxTree)
        {
            HavingKeyword = havingKeyword;
            Predicate = predicate;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.HavingClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return HavingKeyword;
            yield return Predicate;
        }

        public SyntaxToken HavingKeyword { get; }

        public ExpressionSyntax Predicate { get; }
    }
}