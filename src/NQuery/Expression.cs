using NQuery.CodeAnalysis;

namespace NQuery;

public sealed class Expression<T>
{
    private ExpressionEvaluator? _expressionEvaluator;

    private Expression(DataContext dataContext, string text, T nullValue, Type targetType)
    {
        ThrowIfNull(dataContext);
        ThrowIfNull(text);
        ThrowIfNull(targetType);

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
        return Create(dataContext, text, default(T)!);
    }

    public static Expression<T> Create(DataContext dataContext, string text, T nullValue)
    {
        return Create(dataContext, text, nullValue, typeof(T));
    }

    public static Expression<T> Create(DataContext dataContext, string text, T nullValue, Type targetType)
    {
        return new Expression<T>(dataContext, text, nullValue, targetType);
    }

    private ExpressionEvaluator EnsureCompiled()
    {
        if (_expressionEvaluator is null)
        {
            var syntaxTree = SyntaxTree.ParseExpression(Text);
            var compilation = Compilation.Create(DataContext, syntaxTree);
            var compiledQuery = compilation.Compile();
            var expressionEvaluator = compiledQuery.CreateExpressionEvaluator();
            Interlocked.CompareExchange(ref _expressionEvaluator, expressionEvaluator, null);
        }

        return _expressionEvaluator;
    }

    public Type Resolve()
    {
        return EnsureCompiled().Type;
    }

    public T Evaluate()
    {
        var result = EnsureCompiled().Evaluate();
        return result is null
            ? NullValue
            : (T)result;
    }

    public DataContext DataContext { get; }

    public string Text { get; }

    public T NullValue { get; }

    public Type TargetType { get; }
}
