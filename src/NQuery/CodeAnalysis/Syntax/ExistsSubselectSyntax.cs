namespace NQuery.CodeAnalysis.Syntax;

public sealed class ExistsSubselectSyntax : SubselectExpressionSyntax
{
    internal ExistsSubselectSyntax(SyntaxTree syntaxTree, SyntaxToken existsKeyword, SyntaxToken leftParenthesisToken, QuerySyntax query, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        ExistsKeyword = existsKeyword;
        LeftParenthesisToken = leftParenthesisToken;
        Query = query;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.ExistsSubselect; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return ExistsKeyword;
        yield return LeftParenthesisToken;
        yield return Query;
        yield return RightParenthesisToken;
    }

    public SyntaxToken ExistsKeyword { get; }

    public SyntaxToken LeftParenthesisToken { get; }

    public QuerySyntax Query { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
