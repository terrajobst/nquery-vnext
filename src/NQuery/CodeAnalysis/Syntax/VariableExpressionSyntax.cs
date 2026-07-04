namespace NQuery.CodeAnalysis.Syntax;

public sealed class VariableExpressionSyntax : ExpressionSyntax
{
    internal VariableExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken atToken, SyntaxToken identifierToken)
        : base(syntaxTree)
    {
        AtToken = atToken;
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.VariableExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return AtToken;
        yield return IdentifierToken;
    }

    public SyntaxToken AtToken { get; }

    public SyntaxToken IdentifierToken { get; }
}
