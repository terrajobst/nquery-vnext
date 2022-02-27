namespace NQuery.Syntax
{
    public sealed class IsNullExpressionSyntax : ExpressionSyntax
    {
        internal IsNullExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken isKeyword, SyntaxToken notKeyword, SyntaxToken nullKeyword)
            : base(syntaxTree)
        {
            Expression = expression;
            IsKeyword = isKeyword;
            NotKeyword = notKeyword;
            NullKeyword = nullKeyword;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.IsNullExpression; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return Expression;
            yield return IsKeyword;
            if (NotKeyword is not null)
                yield return NotKeyword;
            yield return NullKeyword;
        }

        public ExpressionSyntax Expression { get; }

        public SyntaxToken IsKeyword { get; }

        public SyntaxToken NotKeyword { get; }

        public SyntaxToken NullKeyword { get; }
    }
}