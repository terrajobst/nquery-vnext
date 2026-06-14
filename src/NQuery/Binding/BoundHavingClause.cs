namespace NQuery.Binding;

internal sealed class BoundHavingClause
{
    public BoundHavingClause(BoundExpression condition)
    {
        Condition = condition;
    }

    public BoundExpression Condition { get; }
}
