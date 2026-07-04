namespace NQuery.CodeAnalysis.Syntax;

public sealed class DerivedTableReferenceSyntax : TableReferenceSyntax
{
    internal DerivedTableReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesisToken, QuerySyntax query, SyntaxToken rightParenthesisToken, SyntaxToken? asKeyword, SyntaxToken identifierToken)
        : base(syntaxTree)
    {
        LeftParenthesisToken = leftParenthesisToken;
        Query = query;
        RightParenthesisToken = rightParenthesisToken;
        AsKeyword = asKeyword;
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.DerivedTableReference; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return LeftParenthesisToken;
        yield return Query;
        yield return RightParenthesisToken;
        if (AsKeyword is not null)
            yield return AsKeyword;
        yield return IdentifierToken;
    }

    public SyntaxToken LeftParenthesisToken { get; }

    public QuerySyntax Query { get; }

    public SyntaxToken RightParenthesisToken { get; }

    public SyntaxToken? AsKeyword { get; }

    public SyntaxToken IdentifierToken { get; }
}
