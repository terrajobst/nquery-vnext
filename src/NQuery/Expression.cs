using System;
using System.Threading;

namespace NQuery
{
    public sealed class Expression<T>
    {
        private ExpressionEvaluator _expressionEvaluator;

        private Expression(DataContext dataContext, string text, T nullValue, Type targetType)
        {
            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext));

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (!typeof(T).IsAssignableFrom(targetType))
                throw new ArgumentException($"The target type must be a sub type of {typeof (T).FullName}", nameof(targetType));

            DataContext = dataContext;
            Text = text;
            NullValue = nullValue;
            TargetType = targetType;
        }

        public static Expression<T> Create(DataContext dataContext, string text)
        {
            return Create(dataContext, text, default(T));
        }

        public static Expression<T> Create(DataContext dataContext, string text, T nullValue)
        {
            return Create(dataContext, text, nullValue, typeof(T));
        }

        public static Expression<T> Create(DataContext dataContext, string text, T nullValue, Type targetType)
        {
            return new Expression<T>(dataContext, text, nullValue, targetType);
        }

        private void EnsureCompiled()
        {
            if (_expressionEvaluator != null)
                return;

            var syntaxTree = SyntaxTree.ParseExpression(Text);
            var compilation = Compilation.Create(DataContext, syntaxTree);
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
                ? NullValue
                : (T) result;
        }

        public DataContext DataContext { get; }

        public string Text { get; }

        public T NullValue { get; }

        public Type TargetType { get; }
    }
}