using System;
using System.Threading;

namespace NQuery
{
    public sealed class Expression<T>
    {
        private readonly DataContext _dataContext;
        private readonly string _text;
        private readonly T _nullValue;
        private readonly Type _targetType;

        private ExpressionEvaluator _expressionEvaluator;

        public Expression(DataContext dataContext, string text)
            : this(dataContext, text, default(T))
        {
        }

        public Expression(DataContext dataContext, string text, T nullValue)
            : this(dataContext, text, nullValue, typeof(T))
        {
        }

        public Expression(DataContext dataContext, string text, T nullValue, Type targetType)
        {
            if (dataContext == null)
                throw new ArgumentNullException("dataContext");

            if (text == null)
                throw new ArgumentNullException("text");

            if (targetType == null)
                throw new ArgumentNullException("targetType");

            if (!typeof(T).IsAssignableFrom(targetType))
                throw new ArgumentException($"The target type must be a sub type of {typeof (T).FullName}", "targetType");

            _dataContext = dataContext;
            _text = text;
            _nullValue = nullValue;
            _targetType = targetType;
        }

        private void EnsureCompiled()
        {
            if (_expressionEvaluator != null)
                return;

            var syntaxTree = SyntaxTree.ParseExpression(_text);
            var compilation = new Compilation(_dataContext, syntaxTree);
            var compiledQuery = compilation.Compile();
            var expressionEvaluator = compiledQuery.CreateExpressionEvaluator();
            Interlocked.CompareExchange(ref _expressionEvaluator, expressionEvaluator, null);
        }

        public Type Resolve()
        {
            EnsureCompiled();
            return _expressionEvaluator.Type;
        }

        public T Evaluate()
        {
            EnsureCompiled();
            var result = _expressionEvaluator.Evalutate();
            return result == null
                ? _nullValue
                : (T) result;
        }

        public DataContext DataContext
        {
            get { return _dataContext; }
        }

        public string Text
        {
            get { return _text; }
        }

        public T NullValue
        {
            get { return _nullValue; }
        }

        public Type TargetType
        {
            get { return _targetType; }
        }
    }
}