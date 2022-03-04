namespace NQuery
{
    public sealed class Expression<T>
    {
        private ExpressionEvaluator _expressionEvaluator;

        private Expression(DataContext dataContext, string text, T nullValue, Type targetType)
        {
            if (dataContext is null)
                throw new ArgumentNullException(nameof(dataContext));

            if (text is null)
                throw new ArgumentNullException(nameof(text));

            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            if (!typeof(T).IsAssignableFrom(targetType))
            {
                var message = string.Format(Resources.TargetTypeMismatch, typeof(T), targetType);
                throw new ArgumentException(message, nameof(targetType));
            }

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
            if (_expressionEvaluator is not null)
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
            var result = _expressionEvaluator.Evaluate();
            return result is null
                ? NullValue
                : (T)result;
        }

        public DataContext DataContext { get; }

        public string Text { get; }

        public T NullValue { get; }

        public Type TargetType { get; }
    }
}