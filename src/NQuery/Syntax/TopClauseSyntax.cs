namespace NQuery.Syntax
{
    public sealed class TopClauseSyntax : SyntaxNode
    {
        internal TopClauseSyntax(SyntaxTree syntaxTree, SyntaxToken topKeyword, SyntaxToken value, SyntaxToken withKeyword, SyntaxToken tiesKeyword)
            : base(syntaxTree)
        {
            TopKeyword = topKeyword;
            Value = value;
            WithKeyword = withKeyword;
            TiesKeyword = tiesKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.TopClause; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return TopKeyword;
            yield return Value;
            if (WithKeyword != null)
                yield return WithKeyword;
            if (TiesKeyword != null)
                yield return TiesKeyword;
        }

        public SyntaxToken TopKeyword { get; }

        public SyntaxToken Value { get; }

        public SyntaxToken WithKeyword { get; }

        public SyntaxToken TiesKeyword { get; }
    }
}