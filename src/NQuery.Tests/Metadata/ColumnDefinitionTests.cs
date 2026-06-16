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
    public void ColumnDefinition_ExpressionWithExplicitType_IsQueryable()
    {
        var rows = new[] { new Row(21) };
        var table = TableDefinition.Create("T", rows,
            ColumnDefinition.Create<Row>("Doubled", typeof(int), r => (object)(r.Value * 2)));
        var catalog = Catalog.Empty.AddTables(table);

        var query = Query.Create(catalog, "SELECT Doubled FROM T");
        using var reader = query.ExecuteReader();

        Assert.Equal(typeof(int), reader.GetColumnType(0));
        Assert.True(reader.Read());
        Assert.Equal(42, reader[0]);
    }

    // Mirrors how Emitter.BuildColumnAccess turns a column definition into a row accessor.
    private static Func<object, object> BuildAccessor(ColumnDefinition column)
    {
        var instance = Expression.Parameter(typeof(object));
        var body = column.CreateInvocation(instance);
        return Expression.Lambda<Func<object, object>>(body, instance).Compile();
    }
}
