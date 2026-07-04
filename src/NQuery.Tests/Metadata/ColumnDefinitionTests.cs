using System.Linq.Expressions;

using NQuery.Metadata;

namespace NQuery.Tests.Metadata;

public sealed class ColumnDefinitionTests
{
    private sealed class Row
    {
        public Row(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    [Fact]
    public void ColumnDefinition_Expression_ExposesNameAndDataType()
    {
        var column = ColumnDefinition.Create<Row, int>("Doubled", r => r.Value * 2);

        Assert.Equal("Doubled", column.Name);
        Assert.Equal(typeof(int), column.DataType);
    }

    [Fact]
    public void ColumnDefinition_Expression_ComputesValueFromRow()
    {
        var column = ColumnDefinition.Create<Row, int>("Doubled", r => r.Value * 2);
        var accessor = BuildAccessor(column);

        Assert.Equal(42, accessor(new Row(21)));
    }

    [Fact]
    public void TableDefinition_WithExpressionColumns_IsQueryable()
    {
        var rows = new[] { new Row(1), new Row(2) };
        var table = TableDefinition.Create("Numbers", rows,
            ColumnDefinition.Create<Row, int>("Value", r => r.Value),
            ColumnDefinition.Create<Row, int>("Doubled", r => r.Value * 2));
        var catalog = Catalog.Empty.AddTables(table);

        var query = Query.Create(catalog, "SELECT Value, Doubled FROM Numbers");
        using var reader = query.ExecuteReader();

        Assert.Equal(2, reader.ColumnCount);
        Assert.Equal("Value", reader.GetColumnName(0));
        Assert.Equal(typeof(int), reader.GetColumnType(0));
        Assert.Equal("Doubled", reader.GetColumnName(1));
        Assert.Equal(typeof(int), reader.GetColumnType(1));

        Assert.True(reader.Read());
        Assert.Equal(1, reader[0]);
        Assert.Equal(2, reader[1]);

        Assert.True(reader.Read());
        Assert.Equal(2, reader[0]);
        Assert.Equal(4, reader[1]);

        Assert.False(reader.Read());
    }

    [Fact]
    public void ColumnDefinition_DelegateWithExplicitType_IsQueryable()
    {
        var rows = new[] { new Row(21) };
        var table = TableDefinition.Create("T", rows,
            ColumnDefinition.Create("Doubled", typeof(int), (Func<Row, int>)(r => r.Value * 2)));
        var catalog = Catalog.Empty.AddTables(table);

        var query = Query.Create(catalog, "SELECT Doubled FROM T");
        using var reader = query.ExecuteReader();

        Assert.Equal(typeof(int), reader.GetColumnType(0));
        Assert.True(reader.Read());
        Assert.Equal(42, reader[0]);
    }

    [Fact]
    public void ColumnDefinition_LambdaExpression_WithExplicitType_IsQueryable()
    {
        // A programmatic builder can supply an expression tree without generic type parameters; the
        // body is inlined into the row writer (no per-row delegate call).
        var row = Expression.Parameter(typeof(Row), "row");
        var body = Expression.Multiply(Expression.Property(row, nameof(Row.Value)), Expression.Constant(2));
        var accessor = Expression.Lambda(body, row);

        var rows = new[] { new Row(21) };
        var table = TableDefinition.Create("T", rows,
            ColumnDefinition.Create("Doubled", typeof(int), accessor));
        var catalog = Catalog.Empty.AddTables(table);

        var query = Query.Create(catalog, "SELECT Doubled FROM T");
        using var reader = query.ExecuteReader();

        Assert.Equal(typeof(int), reader.GetColumnType(0));
        Assert.True(reader.Read());
        Assert.Equal(42, reader[0]);
    }

    // CreateInvocation now yields the value in its own CLR type (the row writer lifts it to
    // the typed slot, no boxing); this helper boxes to object for its own object-typed accessor.
    private static Func<object, object> BuildAccessor(ColumnDefinition column)
    {
        var instance = Expression.Parameter(typeof(object));
        var body = Expression.Convert(column.CreateInvocation(instance), typeof(object));
        return Expression.Lambda<Func<object, object>>(body, instance).Compile();
    }
}
