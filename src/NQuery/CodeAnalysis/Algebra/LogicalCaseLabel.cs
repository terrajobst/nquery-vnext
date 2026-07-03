namespace NQuery.CodeAnalysis.Algebra;

internal sealed class LogicalCaseLabel
{
    public LogicalCaseLabel(LogicalExpression condition, LogicalExpression thenExpression)
    {
        ThrowIfNull(condition);
        ThrowIfNull(thenExpression);

        Condition = condition;
        ThenExpression = thenExpression;
    }

    public LogicalExpression Condition { get; }

    public LogicalExpression ThenExpression { get; }
}
