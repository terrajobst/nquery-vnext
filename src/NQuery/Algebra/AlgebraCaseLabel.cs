using System;

namespace NQuery.Algebra
{
    internal sealed class AlgebraCaseLabel
    {
        private readonly AlgebraExpression _condition;
        private readonly AlgebraExpression _result;

        public AlgebraCaseLabel(AlgebraExpression condition, AlgebraExpression result)
        {
            _condition = condition;
            _result = result;
        }

        public AlgebraExpression Condition
        {
            get { return _condition; }
        }

        public AlgebraExpression Result
        {
            get { return _result; }
        }
    }
}