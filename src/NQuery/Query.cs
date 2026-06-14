namespace NQuery;

public sealed class Query
{
    private CompiledQuery? _query;

    private Query(DataContext dataContext, string text)
    {
        DataContext = dataContext;
        Text = text;
    }

    public static Query Create(DataContext dataContext, string text)
    {
        ThrowIfNull(dataContext);
        ThrowIfNull(text);

        return new Query(dataContext, text);
    }

    private CompiledQuery EnsureCompiled()
    {
        var query = _query;
        if (query is not null)
            return query;

        var syntaxTree = SyntaxTree.ParseQuery(Text);
        var compilation = Compilation.Create(DataContext, syntaxTree);
        query = compilation.Compile();
        return Interlocked.CompareExchange(ref _query, query, null) ?? query;
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

    public DataContext DataContext { get; }

    public string Text { get; }
}