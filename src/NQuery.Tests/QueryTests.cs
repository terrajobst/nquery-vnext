using System.Data;

using NQuery.CodeAnalysis;
using NQuery.Data;

namespace NQuery.Tests;

public class QueryTests
{
    [Fact]
    public void Query_AllowsConstructingInvalidQueries()
    {
        var catalog = Catalog.Empty;
        var fooBar = "FOO BAR";

        var query = Query.Create(catalog, fooBar);

        Assert.Equal(catalog, query.Catalog);
        Assert.Equal(fooBar, query.Text);
    }

    [Fact]
    public void Query_CtorThrows_IfCatalogIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            Query.Create(null!, string.Empty);
        });
    }

    [Fact]
    public void Query_CtorThrows_IfTextIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            Query.Create(Catalog.Empty, null!);
        });
    }

    [Fact]
    public void Query_ExecuteSchemaReaderThrows_IfQueryCannotBeParsed()
    {
        var query = Query.Create(Catalog.Empty, "SELECT SchemaReader;");
        try
        {
            query.ExecuteSchemaReader();
            Assert.Fail("Should have thrown an exception.");
        }
        catch (CompilationException ex)
        {
            Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.IllegalInputCharacter));
            Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared));
        }
    }

    [Fact]
    public void Query_ExecuteReaderThrows_IfQueryCannotBeParsed()
    {
        var query = Query.Create(Catalog.Empty, "SELECT Reader;");
        try
        {
            query.ExecuteReader();
            Assert.Fail("Should have thrown an exception.");
        }
        catch (CompilationException ex)
        {
            Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.IllegalInputCharacter));
            Assert.True(ex.Diagnostics.Any(d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared));
        }
    }

    [Fact]
    public void Query_ExecuteScalarOfTThrows_IfTypeDoesNotMatch()
    {
        var dataTable = TestData.IdNameTable();
        var catalog = Catalog.Default.AddTables(dataTable);

        var query = Query.Create(catalog, "SELECT * FROM Table");
        Assert.Throws<InvalidCastException>(() =>
        {
            query.ExecuteScalar<string>();
        });
    }

    [Fact]
    public void Query_ExecuteScalarReturnsFirstValue()
    {
        var dataTable = TestData.IdNameTable();
        var catalog = Catalog.Default.AddTables(dataTable);

        var query = Query.Create(catalog, "SELECT * FROM Table");
        var result = query.ExecuteScalar();
        var expected = dataTable.Rows[0][0];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Query_ExecuteScalarReturnsNullIfEmpty()
    {
        var dataTable = TestData.IdNameTable();
        var catalog = Catalog.Default.AddTables(dataTable);

        var query = Query.Create(catalog, "SELECT * FROM Table WHERE FALSE");
        var result = query.ExecuteScalar();

        Assert.Null(result);
    }

    [Fact]
    public void Query_ExecuteReaderReturnsAllRows()
    {
        var dataTable = new DataTable("Table");
        dataTable.Columns.Add("Value", typeof(int));
        dataTable.Rows.Add(1);
        dataTable.Rows.Add(42);

        var catalog = Catalog.Default.AddTables(dataTable);

        var query = Query.Create(catalog, "SELECT * FROM Table");
        using var reader = query.ExecuteReader();
        Assert.Equal(1, reader.ColumnCount);
        Assert.Equal("Value", reader.GetColumnName(0));
        Assert.Equal(typeof(int), reader.GetColumnType(0));

        Assert.True(reader.Read());
        var first = (int)reader[0];
        Assert.Equal(1, first);

        Assert.True(reader.Read());
        var second = (int)reader[0];
        Assert.Equal(42, second);

        Assert.False(reader.Read());
    }

    [Fact]
    public void Query_ExecuteSchemaReaderReturnsNoRows()
    {
        var dataTable = new DataTable("Table");
        dataTable.Columns.Add("Value", typeof(int));
        dataTable.Rows.Add(1);
        dataTable.Rows.Add(42);

        var catalog = Catalog.Default.AddTables(dataTable);

        var query = Query.Create(catalog, "SELECT * FROM Table");
        using var reader = query.ExecuteSchemaReader();
        Assert.Equal(1, reader.ColumnCount);
        Assert.Equal("Value", reader.GetColumnName(0));
        Assert.Equal(typeof(int), reader.GetColumnType(0));
        Assert.False(reader.Read());
    }

    [Fact]
    public void Query_SelectNullLiteralReturnsObjectNull()
    {
        var text = "SELECT NULL";
        var query = Query.Create(Catalog.Default, text);

        using var queryReader = query.ExecuteReader();
        Assert.Equal(1, queryReader.ColumnCount);
        Assert.Equal(typeof(object), queryReader.GetColumnType(0));

        Assert.True(queryReader.Read());
        Assert.Null(queryReader[0]);
        Assert.False(queryReader.Read());
    }

    [Fact]
    public void Query_SelectNullExpressionReturnsTypedNull()
    {
        var text = "SELECT 1.0 + NULL";
        var query = Query.Create(Catalog.Default, text);

        using var queryReader = query.ExecuteReader();
        Assert.Equal(1, queryReader.ColumnCount);
        Assert.Equal(typeof(double), queryReader.GetColumnType(0));

        Assert.True(queryReader.Read());
        Assert.Null(queryReader[0]);
        Assert.False(queryReader.Read());
    }

    [Fact]
    public void Query_SelectIsNullReturnsBooleanTrue()
    {
        var text = "SELECT NULL IS NULL";
        var query = Query.Create(Catalog.Default, text);

        using var queryReader = query.ExecuteReader();
        Assert.Equal(1, queryReader.ColumnCount);
        Assert.Equal(typeof(bool), queryReader.GetColumnType(0));

        Assert.True(queryReader.Read());
        Assert.Equal(true, queryReader[0]);
        Assert.False(queryReader.Read());
    }
}
