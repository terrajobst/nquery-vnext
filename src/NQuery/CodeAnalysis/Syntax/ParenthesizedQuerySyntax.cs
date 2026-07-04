namespace NQuery.CodeAnalysis.Syntax;

public sealed class ParenthesizedQuerySyntax : QuerySyntax
{
    internal ParenthesizedQuerySyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesisToken, QuerySyntax query, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        LeftParenthesisToken = leftParenthesisToken;
        Query = query;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.ParenthesizedQuery; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return LeftParenthesisToken;
        yield return Query;
        yield return RightParenthesisToken;
    }

    public SyntaxToken LeftParenthesisToken { get; }

    public QuerySyntax Query { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
