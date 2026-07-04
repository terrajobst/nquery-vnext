namespace NQuery.CodeAnalysis.Syntax;

public sealed class InQueryExpressionSyntax : ExpressionSyntax
{
    internal InQueryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken? notKeyword, SyntaxToken inKeyword, SyntaxToken leftParenthesisToken, QuerySyntax query, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        Expression = expression;
        NotKeyword = notKeyword;
        InKeyword = inKeyword;
        LeftParenthesisToken = leftParenthesisToken;
        Query = query;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.InQueryExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return Expression;
        if (NotKeyword is not null)
            yield return NotKeyword;
        yield return InKeyword;
        yield return LeftParenthesisToken;
        yield return Query;
        yield return RightParenthesisToken;
    }

    public ExpressionSyntax Expression { get; }

    public SyntaxToken? NotKeyword { get; }

    public SyntaxToken InKeyword { get; }

    public SyntaxToken LeftParenthesisToken { get; }

    public QuerySyntax Query { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
