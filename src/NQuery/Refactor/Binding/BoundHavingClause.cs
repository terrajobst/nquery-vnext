namespace NQuery.Refactor.Binding
{
    internal sealed class BoundHavingClause
    {
        public BoundHavingClause(BoundExpression condition)
        {
            Condition = condition;
        }

        public BoundExpression Condition { get; }
    }
}
