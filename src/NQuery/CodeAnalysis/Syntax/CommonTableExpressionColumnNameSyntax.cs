namespace NQuery.CodeAnalysis.Syntax;

public sealed class CommonTableExpressionColumnNameSyntax : SyntaxNode
{
    internal CommonTableExpressionColumnNameSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken)
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.CommonTableExpressionColumnName; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return IdentifierToken;
    }

    public SyntaxToken IdentifierToken { get; }
}
