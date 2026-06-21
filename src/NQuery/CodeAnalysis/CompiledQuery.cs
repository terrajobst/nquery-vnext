using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Emit;
using NQuery.CodeAnalysis.Iterators;
using NQuery.CodeAnalysis.Symbols;

namespace NQuery.CodeAnalysis;

// Every compilation -- top-level query and bare expression alike -- is an emitted
// ExecutablePlan. (A bare expression is wrapped in a one-row projection by the algebrizer,
// so it plans and emits like any other query.)
public sealed class CompiledQuery
{
    private readonly ExecutablePlan _plan;
    private readonly ConcurrentDictionary<Type, object> _scalarReaders = new();
    private FrozenDictionary<ValueSlot, RowBufferColumn>? _outputColumnMap;

    internal CompiledQuery(ExecutablePlan plan)
    {
        _plan = plan;
    }

    private ImmutableArray<QueryColumnInstanceSymbol> OutputColumns => _plan.OutputColumns;

    // The output slots resolve to row-buffer columns the same way the reader resolves them; the
    // mapping is derived from the slots alone (not from any one execution's buffer), so it is
    // stable across executions and can back a once-compiled typed reader.
    private FrozenDictionary<ValueSlot, RowBufferColumn> OutputColumnMap =>
        _outputColumnMap ??= RowBufferLayout.CreateSlotMap(_plan.OutputValueSlots);

    public QueryReader CreateReader()
    {
        return CreateReader(false);
    }

    public QueryReader CreateSchemaReader()
    {
        return CreateReader(true);
    }

    private QueryReader CreateReader(bool schemaOnly)
    {
        var columnNamesAndTypes = OutputColumns.Select(c => (c.Name, c.Type.ToOutputType())).ToImmutableArray();
        return new QueryReader(BuildIterator(), columnNamesAndTypes, _plan.OutputValueSlots, schemaOnly);
    }

    private Iterator BuildIterator()
    {
        return _plan.CreateIterator();
    }

    public CompiledExpression CompileExpression()
    {
        // If the query is empty, just return null
        if (OutputColumns.Length == 0)
            return new CompiledExpression(typeof(object), () => null);

        var expressionType = OutputColumns[0].Type;
        return new CompiledExpression(expressionType, EvaluateQueryAsExpression);
    }

    // The strongly-typed evaluator: reads the single output value as T without boxing it
    // through object. SQL NULL (and an empty result) yields default(T).
    public CompiledExpression<T> CompileExpression<T>()
    {
        return CompileExpression<T>(default!);
    }

    // As above, but SQL NULL (and an empty result) yields whenNull -- used by Expression<T>,
    // which substitutes its caller-supplied NullValue.
    public CompiledExpression<T> CompileExpression<T>(T whenNull)
    {
        if (OutputColumns.Length == 0)
            return new CompiledExpression<T>(typeof(T), this, _ => whenNull, whenNull);

        var slot = _plan.OutputValueSlots[0];
        var reader = ColumnReaderCompiler.Compile(OutputColumnMap[slot], slot.Type, whenNull);
        return new CompiledExpression<T>(slot.Type, this, reader, whenNull);
    }

    // Cached per T so repeated Query.ExecuteScalar<T>() calls reuse one compiled reader rather
    // than paying a delegate compile each time.
    internal CompiledExpression<T> ScalarReader<T>()
    {
        return (CompiledExpression<T>)_scalarReaders.GetOrAdd(typeof(T), _ => CompileExpression<T>());
    }

    private object? EvaluateQueryAsExpression()
    {
        // Evaluating an expression means evaluating its query: a bare expression is wrapped
        // into a one-row projection by the algebrizer and emitted like any other query, so we
        // just run that plan and read the single output value.
        using var reader = CreateReader();
        return !reader.Read() || reader.ColumnCount == 0
            ? null
            : reader[0];
    }
}
