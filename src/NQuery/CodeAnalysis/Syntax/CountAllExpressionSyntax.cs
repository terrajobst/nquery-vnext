namespace NQuery.CodeAnalysis.Syntax;

public sealed class CountAllExpressionSyntax : ExpressionSyntax
{
    internal CountAllExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken, SyntaxToken leftParenthesisToken, SyntaxToken asteriskToken, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
        LeftParenthesisToken = leftParenthesisToken;
        AsteriskToken = asteriskToken;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.CountAllExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return IdentifierToken;
        yield return LeftParenthesisToken;
        yield return AsteriskToken;
        yield return RightParenthesisToken;
    }

    public SyntaxToken IdentifierToken { get; }

    public SyntaxToken LeftParenthesisToken { get; }

    public SyntaxToken AsteriskToken { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
