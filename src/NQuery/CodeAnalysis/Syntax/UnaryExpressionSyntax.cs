namespace NQuery.CodeAnalysis.Syntax;

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    internal UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken unaryOperatorToken, ExpressionSyntax expression)
        : base(syntaxTree)
    {
        UnaryOperatorToken = unaryOperatorToken;
        Expression = expression;
    }

    public override SyntaxKind Kind
    {
        get { return SyntaxFacts.GetUnaryOperatorExpression(UnaryOperatorToken.Kind); }
    }

    public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
    {
        yield return UnaryOperatorToken;
        yield return Expression;
    }

    public SyntaxToken UnaryOperatorToken { get; }

    public ExpressionSyntax Expression { get; }
}
