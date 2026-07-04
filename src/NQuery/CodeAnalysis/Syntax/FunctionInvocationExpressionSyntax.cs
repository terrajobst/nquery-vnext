namespace NQuery.CodeAnalysis.Syntax;

public sealed class FunctionInvocationExpressionSyntax : ExpressionSyntax
{
    internal FunctionInvocationExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken, ArgumentListSyntax argumentList)
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
        ArgumentList = argumentList;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.FunctionInvocationExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return IdentifierToken;
        yield return ArgumentList;
    }

    public SyntaxToken IdentifierToken { get; }

    public ArgumentListSyntax ArgumentList { get; }
}
