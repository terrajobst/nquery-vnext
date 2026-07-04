namespace NQuery.CodeAnalysis.Syntax;

public sealed class AllAnySubselectSyntax : SubselectExpressionSyntax
{
    internal AllAnySubselectSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken comparisonOperatorToken, SyntaxToken quantifierKeyword, SyntaxToken leftParenthesisToken, QuerySyntax query, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        Left = left;
        ComparisonOperatorToken = comparisonOperatorToken;
        QuantifierKeyword = quantifierKeyword;
        LeftParenthesisToken = leftParenthesisToken;
        Query = query;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        // TODO: May be we should have different values for ALL, ANY, and SOME?
        get { return SyntaxKind.AllAnySubselect; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return Left;
        yield return ComparisonOperatorToken;
        yield return QuantifierKeyword;
        yield return LeftParenthesisToken;
        yield return Query;
        yield return RightParenthesisToken;
    }

    public ExpressionSyntax Left { get; }

    public SyntaxToken ComparisonOperatorToken { get; }

    public SyntaxToken QuantifierKeyword { get; }

    public SyntaxToken LeftParenthesisToken { get; }

    public QuerySyntax Query { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
