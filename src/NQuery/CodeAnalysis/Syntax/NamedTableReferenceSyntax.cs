namespace NQuery.CodeAnalysis.Syntax;

public sealed class NamedTableReferenceSyntax : TableReferenceSyntax
{
    internal NamedTableReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken, AliasSyntax? alias)
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
        Alias = alias;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.NamedTableReference; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return IdentifierToken;
        if (Alias is not null)
            yield return Alias;
    }

    public SyntaxToken IdentifierToken { get; }

    public AliasSyntax? Alias { get; }
}
