namespace NQuery.CodeAnalysis.Syntax;

public sealed class AliasSyntax : SyntaxNode
{
    internal AliasSyntax(SyntaxTree syntaxTree, SyntaxToken? asKeyword, SyntaxToken identifierToken)
        : base(syntaxTree)
    {
        AsKeyword = asKeyword;
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.Alias; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        if (AsKeyword is not null)
            yield return AsKeyword;
        yield return IdentifierToken;
    }

    public SyntaxToken? AsKeyword { get; }

    public SyntaxToken IdentifierToken { get; }
}
