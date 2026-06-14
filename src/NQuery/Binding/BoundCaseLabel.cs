using NQuery.Binding;

namespace NQuery.Binding
{
    internal sealed class BoundCaseLabel
    {
        public BoundCaseLabel(BoundExpression condition, BoundExpression thenExpression)
        {
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
}