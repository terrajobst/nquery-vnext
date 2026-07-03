namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundCaseLabel
{
    public BoundCaseLabel(BoundExpression condition, BoundExpression thenExpression)
    {
        ThrowIfNull(condition);
        ThrowIfNull(thenExpression);

        Condition = condition;
        ThenExpression = thenExpression;
    }

    public BoundExpression Condition { get; }

    public BoundExpression ThenExpression { get; }

    public override string ToString()
    {
        return $"WHEN {Condition} THEN {ThenExpression}";
    }
}
