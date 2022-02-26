namespace NQuery.Syntax
{
    public abstract class ConditionedJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        internal ConditionedJoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition)
            : base(syntaxTree, left, right)
        {
            OnKeyword = onKeyword;
            Condition = condition;
        }

        public SyntaxToken OnKeyword { get; }

        public ExpressionSyntax Condition { get; }
    }
}