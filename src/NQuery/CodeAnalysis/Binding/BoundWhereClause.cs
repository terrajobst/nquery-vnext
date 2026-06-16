namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundWhereClause
{
    public BoundWhereClause(BoundExpression condition)
    {
        Condition = condition;
    }

    public BoundExpression Condition { get; }
}
