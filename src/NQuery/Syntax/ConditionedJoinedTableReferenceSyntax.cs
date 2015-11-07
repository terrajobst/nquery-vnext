using System;

namespace NQuery.Syntax
{
    public abstract class ConditionedJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        private readonly SyntaxToken _onKeyword;
        private readonly ExpressionSyntax _condition;

        internal ConditionedJoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, TableReferenceSyntax right, SyntaxToken onKeyword, ExpressionSyntax condition)
            : base(syntaxTree, left, right)
        {
            _onKeyword = onKeyword;
            _condition = condition;
        }

        public SyntaxToken OnKeyword
        {
            get { return _onKeyword; }
        }

        public ExpressionSyntax Condition
        {
            get { return _condition; }
        }
    }
}