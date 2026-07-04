namespace NQuery.CodeAnalysis.Syntax;

public sealed class ParenthesizedTableReferenceSyntax : TableReferenceSyntax
{
    internal ParenthesizedTableReferenceSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesisToken, TableReferenceSyntax tableReference, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        LeftParenthesisToken = leftParenthesisToken;
        TableReference = tableReference;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.ParenthesizedTableReference; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return LeftParenthesisToken;
        yield return TableReference;
        yield return RightParenthesisToken;
    }

    public SyntaxToken LeftParenthesisToken { get; }

    public TableReferenceSyntax TableReference { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
