namespace NQuery.Syntax
{
    public sealed class CaseElseLabelSyntax : SyntaxNode
    {
        internal CaseElseLabelSyntax(SyntaxTree syntaxTree, SyntaxToken elseKeyword, ExpressionSyntax expression)
            : base(syntaxTree)
        {
            ElseKeyword = elseKeyword;
            Expression = expression;
        }

        public override SyntaxKind Kind
        {
            get { return SyntaxKind.CaseElseLabel; }
        }

        public override IEnumerable<SyntaxNodeOrToken> ChildNodesAndTokens()
        {
            yield return ElseKeyword;
            yield return Expression;
        }

        public SyntaxToken ElseKeyword { get; }

        public ExpressionSyntax Expression { get; }
    }
}