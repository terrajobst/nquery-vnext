namespace NQuery.CodeAnalysis.Syntax;

public sealed class MethodInvocationExpressionSyntax : ExpressionSyntax
{
    internal MethodInvocationExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax target, SyntaxToken dotToken, SyntaxToken identifierToken, ArgumentListSyntax argumentList)
        : base(syntaxTree)
    {
        Target = target;
        DotToken = dotToken;
        IdentifierToken = identifierToken;
        ArgumentList = argumentList;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.MethodInvocationExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return Target;
        yield return DotToken;
        yield return IdentifierToken;
        yield return ArgumentList;
    }

    public ExpressionSyntax Target { get; }

    public SyntaxToken DotToken { get; }

    public SyntaxToken IdentifierToken { get; }

    public ArgumentListSyntax ArgumentList { get; }
}
