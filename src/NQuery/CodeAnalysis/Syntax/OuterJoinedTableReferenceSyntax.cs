namespace NQuery.CodeAnalysis.Syntax;

public sealed class OuterJoinedTableReferenceSyntax : ConditionedJoinedTableReferenceSyntax
{
    internal OuterJoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, SyntaxToken joinTypeKeyword, SyntaxToken? outerKeyword, SyntaxToken joinKeyword, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition)
        : base(syntaxTree, left, right, onKeyword, condition)
    {
        JoinTypeKeyword = joinTypeKeyword;
        OuterKeyword = outerKeyword;
        JoinKeyword = joinKeyword;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxKind.OuterJoinedTableReference; }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return Left;
        yield return JoinTypeKeyword;
        if (OuterKeyword is not null)
            yield return OuterKeyword;
        yield return JoinKeyword;
        yield return Right;
        yield return OnKeyword;
        yield return Condition;
    }

    public SyntaxToken JoinTypeKeyword { get; }

    public SyntaxToken? OuterKeyword { get; }

    public SyntaxToken JoinKeyword { get; }
}
