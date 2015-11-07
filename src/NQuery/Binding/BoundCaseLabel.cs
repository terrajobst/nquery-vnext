using System;

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

        public BoundCaseLabel Update(BoundExpression condition, BoundExpression thenExpression)
        {
            if (condition == Condition && thenExpression == ThenExpression)
                return this;

            return new BoundCaseLabel(condition, thenExpression);
        }

        public override string ToString()
        {
            return $"WHEN {Condition} THEN {ThenExpression}";
        }
    }
}