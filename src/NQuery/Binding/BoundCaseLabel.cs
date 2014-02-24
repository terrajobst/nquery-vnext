using System;

namespace NQuery.Binding
{
    internal sealed class BoundCaseLabel
    {
        private readonly BoundExpression _condition;
        private readonly BoundExpression _thenExpression;

        public BoundCaseLabel(BoundExpression condition, BoundExpression thenExpression)
        {
            _condition = condition;
            _thenExpression = thenExpression;
        }

        public BoundExpression Condition
        {
            get { return _condition; }
        }

        public BoundExpression ThenExpression
        {
            get { return _thenExpression; }
        }

        public override string ToString()
        {
            return string.Format("WHEN {0} THEN {1}", _condition, _thenExpression);
        }
    }
}