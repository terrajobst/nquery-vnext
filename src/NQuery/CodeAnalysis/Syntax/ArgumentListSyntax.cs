namespace NQuery.CodeAnalysis.Syntax;

public sealed class ArgumentListSyntax : SyntaxNode
{
    internal ArgumentListSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesisToken, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        LeftParenthesisToken = leftParenthesisToken;
        Arguments = arguments;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.ArgumentList; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return LeftParenthesisToken;

        foreach (var nodeOrToken in Arguments.GetWithSeparators())
            yield return nodeOrToken;

        yield return RightParenthesisToken;
    }

    public SyntaxToken LeftParenthesisToken { get; }

    public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
