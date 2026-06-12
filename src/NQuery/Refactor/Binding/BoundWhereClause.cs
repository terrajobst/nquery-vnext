namespace NQuery.Refactor.Binding
{
    internal sealed class BoundWhereClause
    {
        public BoundWhereClause(BoundExpression condition)
        {
            Condition = condition;
        }

        public BoundExpression Condition { get; }
    }
}
