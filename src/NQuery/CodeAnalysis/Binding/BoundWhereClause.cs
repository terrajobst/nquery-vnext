namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundWhereClause
{
    public BoundWhereClause(BoundExpression condition)
    {
        ThrowIfNull(condition);

        Condition = condition;
    }

    public BoundExpression Condition { get; }
}
