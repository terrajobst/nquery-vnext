namespace NQuery.CodeAnalysis.Syntax;

public sealed class OrderByColumnSyntax : SyntaxNode
{
    internal OrderByColumnSyntax(SyntaxTree syntaxTree, ExpressionSyntax columnSelector, SyntaxToken? sortDirectionKeyword)
        : base(syntaxTree)
    {
        ColumnSelector = columnSelector;
        SortDirectionKeyword = sortDirectionKeyword;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.OrderByColumn; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return ColumnSelector;
        if (SortDirectionKeyword is not null)
            yield return SortDirectionKeyword;
    }

    public ExpressionSyntax ColumnSelector { get; }

    public SyntaxToken? SortDirectionKeyword { get; }
}
