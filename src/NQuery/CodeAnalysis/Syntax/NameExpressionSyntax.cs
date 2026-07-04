namespace NQuery.CodeAnalysis.Syntax;

public sealed class NameExpressionSyntax : ExpressionSyntax
{
    internal NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken)
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.NameExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return IdentifierToken;
    }

    public SyntaxToken IdentifierToken { get; }
}
