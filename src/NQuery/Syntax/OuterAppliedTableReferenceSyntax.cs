namespace NQuery.Syntax;

public sealed class OuterAppliedTableReferenceSyntax : JoinedTableReferenceSyntax
{
    internal OuterAppliedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, SyntaxToken outerKeyword, SyntaxToken applyKeyword, TableReferenceSyntax right)
        : base(syntaxTree, left, right)
    {
        OuterKeyword = outerKeyword;
        ApplyKeyword = applyKeyword;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.OuterAppliedTableReference; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return Left;
        yield return OuterKeyword;
        yield return ApplyKeyword;
        yield return Right;
    }

    public SyntaxToken OuterKeyword { get; }

    public SyntaxToken ApplyKeyword { get; }
}
