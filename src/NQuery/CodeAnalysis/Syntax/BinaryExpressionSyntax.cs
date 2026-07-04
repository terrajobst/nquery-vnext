namespace NQuery.CodeAnalysis.Syntax;

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    internal BinaryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken binaryOperatorToken, ExpressionSyntax right)
        : base(syntaxTree)
    {
        Left = left;
        BinaryOperatorToken = binaryOperatorToken;
        Right = right;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxFacts.GetBinaryOperatorExpression(BinaryOperatorToken.Kind); }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return Left;
        yield return BinaryOperatorToken;
        yield return Right;
    }

    public ExpressionSyntax Left { get; }

    public SyntaxToken BinaryOperatorToken { get; }

    public ExpressionSyntax Right { get; }
}
