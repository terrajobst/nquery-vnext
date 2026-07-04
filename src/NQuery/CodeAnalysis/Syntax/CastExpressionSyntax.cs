namespace NQuery.CodeAnalysis.Syntax;

public sealed class CastExpressionSyntax : ExpressionSyntax
{
    internal CastExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken castKeyword, SyntaxToken leftParenthesisToken, ExpressionSyntax expression, SyntaxToken asKeyword, SyntaxToken identifierToken, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        CastKeyword = castKeyword;
        LeftParenthesisToken = leftParenthesisToken;
        Expression = expression;
        AsKeyword = asKeyword;
        IdentifierToken = identifierToken;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.CastExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return CastKeyword;
        yield return LeftParenthesisToken;
        yield return Expression;
        yield return AsKeyword;
        yield return IdentifierToken;
        yield return RightParenthesisToken;
    }

    public SyntaxToken CastKeyword { get; }

    public SyntaxToken LeftParenthesisToken { get; }

    public ExpressionSyntax Expression { get; }

    public SyntaxToken AsKeyword { get; }

    public SyntaxToken IdentifierToken { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
