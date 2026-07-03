namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundHavingClause
{
    public BoundHavingClause(BoundExpression condition)
    {
        ThrowIfNull(condition);

        Condition = condition;
    }

    public BoundExpression Condition { get; }
}
