using NQuery.CodeAnalysis.Iterators;

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

// The strongly-typed counterpart of CompiledExpression: Evaluate() returns T directly, reading
// the single output column through a delegate compiled once (see ColumnReaderCompiler) instead
// of boxing the value to object and casting it back. SQL NULL (and an empty result) yields the
// whenNull value baked into the reader.
public sealed class CompiledExpression<T>
{
    private readonly CompiledQuery _query;
    private readonly Func<RowBuffer, T> _reader;
    private readonly T _whenNull;

    internal CompiledExpression(Type type, CompiledQuery query, Func<RowBuffer, T> reader, T whenNull)
    {
        Type = type;
        _query = query;
        _reader = reader;
        _whenNull = whenNull;
    }

    public Type Type { get; }

    public T Evaluate()
    {
        using var reader = _query.CreateReader();
        return reader.Read() && reader.ColumnCount > 0
            ? reader.ReadField(0, _reader)
            : _whenNull;
    }
}
