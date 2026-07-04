namespace NQuery.CodeAnalysis.Syntax;

public sealed class CommonTableExpressionSyntax : SyntaxNode
{
    internal CommonTableExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken? recursiveKeyword, SyntaxToken identifierToken, CommonTableExpressionColumnNameListSyntax? columnNameList, SyntaxToken asKeyword, SyntaxToken leftParenthesisToken, QuerySyntax query, SyntaxToken rightParenthesisToken)
        : base(syntaxTree)
    {
        RecursiveKeyword = recursiveKeyword;
        IdentifierToken = identifierToken;
        ColumnNameList = columnNameList;
        AsKeyword = asKeyword;
        LeftParenthesisToken = leftParenthesisToken;
        Query = query;
        RightParenthesisToken = rightParenthesisToken;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.CommonTableExpression; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        if (RecursiveKeyword is not null)
            yield return RecursiveKeyword;
        yield return IdentifierToken;
        if (ColumnNameList is not null)
            yield return ColumnNameList;
        yield return AsKeyword;
        yield return LeftParenthesisToken;
        yield return Query;
        yield return RightParenthesisToken;
    }

    public SyntaxToken? RecursiveKeyword { get; }

    public SyntaxToken IdentifierToken { get; }

    public CommonTableExpressionColumnNameListSyntax? ColumnNameList { get; }

    public SyntaxToken AsKeyword { get; }

    public SyntaxToken LeftParenthesisToken { get; }

    public QuerySyntax Query { get; }

    public SyntaxToken RightParenthesisToken { get; }
}
