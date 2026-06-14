namespace NQuery.Syntax;

public sealed class CrossAppliedTableReferenceSyntax : JoinedTableReferenceSyntax
{
    internal CrossAppliedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, SyntaxToken crossKeyword, SyntaxToken applyKeyword, TableReferenceSyntax right)
        : base(syntaxTree, left, right)
    {
        CrossKeyword = crossKeyword;
        ApplyKeyword = applyKeyword;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.CrossAppliedTableReference; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return Left;
        yield return CrossKeyword;
        yield return ApplyKeyword;
        yield return Right;
    }

    public SyntaxToken CrossKeyword { get; }

    public SyntaxToken ApplyKeyword { get; }
}
