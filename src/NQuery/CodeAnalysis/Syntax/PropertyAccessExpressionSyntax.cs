namespace NQuery.CodeAnalysis.Syntax;

public sealed class PropertyAccessExpressionSyntax : ExpressionSyntax
{
    internal PropertyAccessExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax target, SyntaxToken dotToken, SyntaxToken identifierToken)
        : base(syntaxTree)
    {
        Target = target;
        DotToken = dotToken;
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.PropertyAccessExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return Target;
        yield return DotToken;
        yield return IdentifierToken;
    }

    public ExpressionSyntax Target { get; }

    public SyntaxToken DotToken { get; }

    public SyntaxToken IdentifierToken { get; }
}
