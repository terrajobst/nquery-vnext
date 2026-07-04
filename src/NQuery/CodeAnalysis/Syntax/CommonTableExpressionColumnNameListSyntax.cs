namespace NQuery.CodeAnalysis.Syntax;

public sealed class CommonTableExpressionColumnNameListSyntax : SyntaxNode
{
    internal CommonTableExpressionColumnNameListSyntax(SyntaxTree syntaxTree, SyntaxToken leftParenthesisToken, SeparatedSyntaxList<CommonTableExpressionColumnNameSyntax> columnNames, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        LeftParenthesisToken = leftParenthesisToken;
        ColumnNames = columnNames;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.CommonTableExpressionColumnNameList; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return LeftParenthesisToken;
        foreach (var nodeOrToken in ColumnNames.GetWithSeparators())
            yield return nodeOrToken;
        yield return RightParenthesisToken;
    }

    public SyntaxToken LeftParenthesisToken { get; }

    public SeparatedSyntaxList<CommonTableExpressionColumnNameSyntax> ColumnNames { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
