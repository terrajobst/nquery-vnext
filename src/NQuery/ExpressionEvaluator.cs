using System;

namespace NQuery
{
    public sealed class ExpressionEvaluator
    {
        private readonly Type _type;
        private readonly Func<object> _evaluator;

        public ExpressionEvaluator(Type type, Func<object> evaluator)
        {
            _type = type;
            _evaluator = evaluator;
        }

        public Type Type
        {
            get { return _type; }
        }

        public object Evalutate()
        {
            return _evaluator();
        }
    }
}