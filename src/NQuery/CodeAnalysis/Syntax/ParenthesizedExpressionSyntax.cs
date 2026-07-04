namespace NQuery.CodeAnalysis.Syntax;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    internal ParenthesizedExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesisToken, ExpressionSyntax expression, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        LeftParenthesisToken = leftParenthesisToken;
        Expression = expression;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.ParenthesizedExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return LeftParenthesisToken;
        yield return Expression;
        yield return RightParenthesisToken;
    }

    public SyntaxToken LeftParenthesisToken { get; }

    public ExpressionSyntax Expression { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
