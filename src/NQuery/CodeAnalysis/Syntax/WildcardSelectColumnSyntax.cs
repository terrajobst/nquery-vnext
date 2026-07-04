namespace NQuery.CodeAnalysis.Syntax;

public sealed class WildcardSelectColumnSyntax : SelectColumnSyntax
{
    internal WildcardSelectColumnSyntax(SyntaxTree syntaxTree, SyntaxToken? identifierToken, SyntaxToken? dotToken, SyntaxToken asteriskToken)
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
        DotToken = dotToken;
        AsteriskToken = asteriskToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.WildcardSelectColumn; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        if (IdentifierToken is not null)
            yield return IdentifierToken;
        if (DotToken is not null)
            yield return DotToken;
        yield return AsteriskToken;
    }

    public SyntaxToken? IdentifierToken { get; }

    public SyntaxToken? DotToken { get; }

    public SyntaxToken AsteriskToken { get; }
}
