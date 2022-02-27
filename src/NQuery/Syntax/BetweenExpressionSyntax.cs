namespace NQuery.Syntax
{
    public sealed class BetweenExpressionSyntax : ExpressionSyntax
    {
        internal BetweenExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken notKeyword, SyntaxToken betweenKeyword, ExpressionSyntax lowerBound, SyntaxToken andKeyword, ExpressionSyntax upperBound)
            : base(syntaxTree)
        {
            Left = left;
            NotKeyword = notKeyword;
            BetweenKeyword = betweenKeyword;
            LowerBound = lowerBound;
            AndKeyword = andKeyword;
            UpperBound = upperBound;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.BetweenExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            if (NotKeyword is not null)
                yield return NotKeyword;
            yield return BetweenKeyword;
            yield return LowerBound;
            yield return AndKeyword;
            yield return UpperBound;
        }

        public ExpressionSyntax Left { get; }

        public SyntaxToken NotKeyword { get; }

        public SyntaxToken BetweenKeyword { get; }

        public ExpressionSyntax LowerBound { get; }

        public SyntaxToken AndKeyword { get; }

        public ExpressionSyntax UpperBound { get; }
    }
}