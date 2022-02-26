namespace NQuery.Syntax
{
    public sealed class NullIfExpressionSyntax : ExpressionSyntax
    {
        internal NullIfExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken nullIfKeyword, SyntaxToken leftParenthesisToken, ExpressionSyntax leftExpression, SyntaxToken commaToken, ExpressionSyntax rightExpression, SyntaxToken rightParenthesisToken)
            : base(syntaxTree)
        {
            NullIfKeyword = nullIfKeyword;
            LeftParenthesisToken = leftParenthesisToken;
            LeftExpression = leftExpression;
            CommaToken = commaToken;
            RightExpression = rightExpression;
            RightParenthesisToken = rightParenthesisToken;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.NullIfExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return NullIfKeyword;
            yield return LeftParenthesisToken;
            yield return LeftExpression;
            yield return CommaToken;
            yield return RightExpression;
            yield return RightParenthesisToken;
        }

        public SyntaxToken NullIfKeyword { get; }

        public SyntaxToken LeftParenthesisToken { get; }

        public ExpressionSyntax LeftExpression { get; }

        public SyntaxToken CommaToken { get; }

        public ExpressionSyntax RightExpression { get; }

        public SyntaxToken RightParenthesisToken { get; }
    }
}