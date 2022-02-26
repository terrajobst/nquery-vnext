namespace NQuery.Syntax
{
    public sealed class AliasSyntax : SyntaxNode
    {
        internal AliasSyntax(SyntaxTree syntaxTree, SyntaxToken asKeyword, SyntaxToken identifier)
            : base(syntaxTree)
        {
            AsKeyword = asKeyword;
            Identifier = identifier;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.Alias; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            if (AsKeyword != null)
                yield return AsKeyword;
            yield return Identifier;
        }

        public SyntaxToken AsKeyword { get; }

        public SyntaxToken Identifier { get; }
    }
}