namespace NQuery.CodeAnalysis.Syntax;

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    internal LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken, object? value)
        : base(syntaxTree)
    {
        LiteralToken = literalToken;
        Value = value;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.LiteralExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return LiteralToken;
    }

    public SyntaxToken LiteralToken { get; }

    public object? Value { get; }
}
