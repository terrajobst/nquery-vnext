namespace NQuery.Syntax
{
    public sealed class SimilarToExpressionSyntax : ExpressionSyntax
    {
        internal SimilarToExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken notKeyword, SyntaxToken similarKeyword, SyntaxToken toKeyword, ExpressionSyntax right)
            : base(syntaxTree)
        {
            Left = left;
            NotKeyword = notKeyword;
            SimilarKeyword = similarKeyword;
            ToKeyword = toKeyword;
            Right = right;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.SimilarToExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Left;
            if (NotKeyword != null)
                yield return NotKeyword;
            yield return SimilarKeyword;
            yield return ToKeyword;
            yield return Right;
        }

        public ExpressionSyntax Left { get; }

        public SyntaxToken NotKeyword { get; }

        public SyntaxToken SimilarKeyword { get; }

        public SyntaxToken ToKeyword { get; }

        public ExpressionSyntax Right { get; }
    }
}