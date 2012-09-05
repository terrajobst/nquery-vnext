using System;

namespace NQuery.Language
{
    public abstract class ConditionedJoinedTableReferenceSyntax : JoinedTableReferenceSyntax
    {
        private readonly ExpressionSyntax _condition;

        protected ConditionedJoinedTableReferenceSyntax(SyntaxTree syntaxTree, TableReferenceSyntax left, TableReferenceSyntax right, ExpressionSyntax condition, SyntaxToken? commaToken)
            : base(syntaxTree, left, right, commaToken)
        {
            _condition = condition;
        }

        public ExpressionSyntax Condition
        {
            get { return _condition; }
        }
    }
}