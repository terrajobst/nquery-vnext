#nullable enable

namespace NQuery.Algebra
{
    internal sealed class LogicalCaseLabel
    {
        public LogicalCaseLabel(LogicalExpression condition, LogicalExpression thenExpression)
        {
            Condition = condition;
            ThenExpression = thenExpression;
        }

        public LogicalExpression Condition { get; }

        public LogicalExpression ThenExpression { get; }
    }
}
