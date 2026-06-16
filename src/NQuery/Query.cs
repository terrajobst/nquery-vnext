using NQuery.CodeAnalysis;

namespace NQuery;

public sealed class Query
{
    private CompiledQuery? _query;

    private Query(Catalog catalog, string text)
    {
        Catalog = catalog;
        Text = text;
    }

    public static Query Create(Catalog catalog, string text)
    {
        ThrowIfNull(catalog);
        ThrowIfNull(text);

        return new Query(catalog, text);
    }

    private CompiledQuery EnsureCompiled()
    {
        if (_query is null)
        {
            var syntaxTree = SyntaxTree.ParseQuery(Text);
            var compilation = Compilation.Create(Catalog, syntaxTree);
            var query = compilation.Compile();
            Interlocked.CompareExchange(ref _query, query, null);
        }

        return _query;
    }

    public object? ExecuteScalar()
    {
        using var reader = ExecuteReader();
        return !reader.Read() || reader.ColumnCount == 0
            ? null
            : reader[0];
    }

    public T ExecuteScalar<T>()
    {
        return (T)ExecuteScalar()!;
    }

    public QueryReader ExecuteReader()
    {
        return EnsureCompiled().CreateReader();
    }

    public QueryReader ExecuteSchemaReader()
    {
        return EnsureCompiled().CreateSchemaReader();
    }

    public Catalog Catalog { get; }

    public string Text { get; }
}
