namespace NQuery.CodeAnalysis;

public sealed class CompiledExpression
{
    private readonly Func<object?> _evaluator;

    internal CompiledExpression(Type type, Func<object?> evaluator)
    {
        Type = type;
        _evaluator = evaluator;
    }

    public Type Type { get; }

    public object? Evaluate()
    {
        return _evaluator();
    }
}
