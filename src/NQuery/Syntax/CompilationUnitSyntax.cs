namespace NQuery.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        internal CompilationUnitSyntax(SyntaxTree syntaxTree, SyntaxNode root, SyntaxToken endOfFileToken)
            : base(syntaxTree)
        {
            Root = root;
            EndOfFileToken = endOfFileToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CompilationUnit; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            if (Root is not null)
                yield return Root;
            yield return EndOfFileToken;
        }

        public SyntaxNode Root { get; }

        public SyntaxToken EndOfFileToken { get; }
    }
}