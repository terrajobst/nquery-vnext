using System;

namespace NQuery
{
    public sealed class ExpressionEvaluator
    {
        private readonly Func<object> _evaluator;

        public ExpressionEvaluator(Type type, Func<object> evaluator)
        {
            Type = type;
            _evaluator = evaluator;
        }

        public Type Type { get; }

        public object Evalutate()
        {
            return _evaluator();
        }
    }
}