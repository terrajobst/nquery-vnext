using NQuery.Metadata;

namespace NQuery.Tests.Metadata;

// Covers both construction paths -- the strongly-typed VariableDefinition<T> and the object-typed
// VariableDefinition.Create(name, type, value) -- across a non-nullable value type (int), a nullable
// value type (int?), a reference type (string), and a nullable reference type (string?). For each we
// check the value round-trips through the definition and that `SELECT @p` reports the erased column
// type and returns the live value.
//
// Nullable<T> and nullable reference annotations are erased to the underlying type at the metadata
// boundary, so the reported column type is int for both int/int? and string for both string/string?.
public sealed class VariableDefinitionTests
{
    private static object? SelectValue(VariableDefinition variable)
    {
        var catalog = Catalog.Default.AddVariables(variable);
        var query = Query.Create(catalog, "SELECT @p");
        using var reader = query.ExecuteReader();

        Assert.True(reader.Read());
        var value = reader[0];
        Assert.False(reader.Read());
        return value;
    }

    private static Type SelectColumnType(VariableDefinition variable)
    {
        var catalog = Catalog.Default.AddVariables(variable);
        var query = Query.Create(catalog, "SELECT @p");
        using var reader = query.ExecuteReader();

        return reader.GetColumnType(0);
    }

    // VariableDefinition<int>

    [Fact]
    public void Generic_Int_CanSetAndReadValue()
    {
        var variable = new VariableDefinition<int>("p");

        variable.Value = 42;

        Assert.Equal(42, variable.Value);
    }

    [Fact]
    public void Generic_Int_SelectReturnsValue()
    {
        var variable = VariableDefinition.Create<int>("p", 42);

        Assert.Equal(42, SelectValue(variable));
    }

    [Fact]
    public void Generic_Int_ReportsColumnType()
    {
        var variable = VariableDefinition.Create<int>("p", 42);

        Assert.Equal(typeof(int), variable.Type);
        Assert.Equal(typeof(int), SelectColumnType(variable));
    }

    // VariableDefinition<int?>

    [Fact]
    public void Generic_NullableInt_CanSetAndReadNonNullValue()
    {
        var variable = new VariableDefinition<int?>("p");

        variable.Value = 42;

        Assert.Equal(42, variable.Value);
    }

    [Fact]
    public void Generic_NullableInt_SelectReturnsNonNullValue()
    {
        var variable = VariableDefinition.Create<int?>("p", 42);

        Assert.Equal(42, SelectValue(variable));
    }

    [Fact]
    public void Generic_NullableInt_SelectReturnsNullValue()
    {
        var variable = VariableDefinition.Create<int?>("p", null);

        Assert.Null(SelectValue(variable));
    }

    [Fact]
    public void Generic_NullableInt_ReportsColumnType()
    {
        var variable = VariableDefinition.Create<int?>("p", 42);

        Assert.Equal(typeof(int), variable.Type);
        Assert.Equal(typeof(int), SelectColumnType(variable));
    }

    // VariableDefinition<string>

    [Fact]
    public void Generic_String_CanSetAndReadValue()
    {
        var variable = new VariableDefinition<string>("p");

        variable.Value = "hello";

        Assert.Equal("hello", variable.Value);
    }

    [Fact]
    public void Generic_String_SelectReturnsValue()
    {
        var variable = VariableDefinition.Create<string>("p", "hello");

        Assert.Equal("hello", SelectValue(variable));
    }

    [Fact]
    public void Generic_String_ReportsColumnType()
    {
        var variable = VariableDefinition.Create<string>("p", "hello");

        Assert.Equal(typeof(string), variable.Type);
        Assert.Equal(typeof(string), SelectColumnType(variable));
    }

    // VariableDefinition<string?>

    [Fact]
    public void Generic_NullableString_CanSetAndReadNonNullValue()
    {
        var variable = new VariableDefinition<string?>("p");

        variable.Value = "hello";

        Assert.Equal("hello", variable.Value);
    }

    [Fact]
    public void Generic_NullableString_SelectReturnsNonNullValue()
    {
        var variable = VariableDefinition.Create<string?>("p", "hello");

        Assert.Equal("hello", SelectValue(variable));
    }

    [Fact]
    public void Generic_NullableString_SelectReturnsNullValue()
    {
        var variable = VariableDefinition.Create<string?>("p", null);

        Assert.Null(SelectValue(variable));
    }

    [Fact]
    public void Generic_NullableString_ReportsColumnType()
    {
        var variable = VariableDefinition.Create<string?>("p", "hello");

        Assert.Equal(typeof(string), variable.Type);
        Assert.Equal(typeof(string), SelectColumnType(variable));
    }

    // VariableDefinition.Create(name, typeof(int), value)

    [Fact]
    public void NonGeneric_Int_CanSetAndReadValue()
    {
        var variable = VariableDefinition.Create("p", typeof(int));

        variable.Value = 42;

        Assert.Equal(42, variable.Value);
    }

    [Fact]
    public void NonGeneric_Int_SelectReturnsValue()
    {
        var variable = VariableDefinition.Create("p", typeof(int), 42);

        Assert.Equal(42, SelectValue(variable));
    }

    [Fact]
    public void NonGeneric_Int_ReportsColumnType()
    {
        var variable = VariableDefinition.Create("p", typeof(int), 42);

        Assert.Equal(typeof(int), variable.Type);
        Assert.Equal(typeof(int), SelectColumnType(variable));
    }

    // VariableDefinition.Create(name, typeof(int?), value)

    [Fact]
    public void NonGeneric_NullableInt_CanSetAndReadNonNullValue()
    {
        var variable = VariableDefinition.Create("p", typeof(int?));

        variable.Value = 42;

        Assert.Equal(42, variable.Value);
    }

    [Fact]
    public void NonGeneric_NullableInt_SelectReturnsNonNullValue()
    {
        var variable = VariableDefinition.Create("p", typeof(int?), 42);

        Assert.Equal(42, SelectValue(variable));
    }

    [Fact]
    public void NonGeneric_NullableInt_SelectReturnsNullValue()
    {
        var variable = VariableDefinition.Create("p", typeof(int?), null);

        Assert.Null(SelectValue(variable));
    }

    [Fact]
    public void NonGeneric_NullableInt_ReportsColumnType()
    {
        var variable = VariableDefinition.Create("p", typeof(int?), 42);

        Assert.Equal(typeof(int), variable.Type);
        Assert.Equal(typeof(int), SelectColumnType(variable));
    }

    // VariableDefinition.Create(name, typeof(string), value)

    [Fact]
    public void NonGeneric_String_CanSetAndReadValue()
    {
        var variable = VariableDefinition.Create("p", typeof(string));

        variable.Value = "hello";

        Assert.Equal("hello", variable.Value);
    }

    [Fact]
    public void NonGeneric_String_SelectReturnsValue()
    {
        var variable = VariableDefinition.Create("p", typeof(string), "hello");

        Assert.Equal("hello", SelectValue(variable));
    }

    [Fact]
    public void NonGeneric_String_ReportsColumnType()
    {
        var variable = VariableDefinition.Create("p", typeof(string), "hello");

        Assert.Equal(typeof(string), variable.Type);
        Assert.Equal(typeof(string), SelectColumnType(variable));
    }

    // VariableDefinition.Create(name, typeof(string), value) -- string? has no distinct runtime Type
    // (typeof(string?) is not expressible), so the nullable reference case uses typeof(string).

    [Fact]
    public void NonGeneric_NullableString_CanSetAndReadNonNullValue()
    {
        var variable = VariableDefinition.Create("p", typeof(string));

        variable.Value = "hello";

        Assert.Equal("hello", variable.Value);
    }

    [Fact]
    public void NonGeneric_NullableString_SelectReturnsNonNullValue()
    {
        var variable = VariableDefinition.Create("p", typeof(string), "hello");

        Assert.Equal("hello", SelectValue(variable));
    }

    [Fact]
    public void NonGeneric_NullableString_SelectReturnsNullValue()
    {
        var variable = VariableDefinition.Create("p", typeof(string), null);

        Assert.Null(SelectValue(variable));
    }

    [Fact]
    public void NonGeneric_NullableString_ReportsColumnType()
    {
        var variable = VariableDefinition.Create("p", typeof(string), "hello");

        Assert.Equal(typeof(string), variable.Type);
        Assert.Equal(typeof(string), SelectColumnType(variable));
    }
}
