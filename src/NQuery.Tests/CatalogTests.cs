namespace NQuery.Tests;

public class CatalogTests
{
    [Fact]
    public void Catalog_AddingNoTablesYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after1 = before.AddTables();
        var after2 = before.AddTables(null!);
        Assert.Same(before, after1);
        Assert.Same(before, after2);
    }

    [Fact]
    public void Catalog_ReplacingWithSameTablesYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after1 = before.WithTables(before.Tables);
        Assert.Same(before, after1);
    }

    [Fact]
    public void Catalog_AddingNoRelationsYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after1 = before.AddRelationships();
        var after2 = before.AddRelationships(null!);
        Assert.Same(before, after1);
        Assert.Same(before, after2);
    }

    [Fact]
    public void Catalog_ReplacingWithSameRelationsYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after = before.WithRelationships(before.Relationships);
        Assert.Same(before, after);
    }

    [Fact]
    public void Catalog_AddingNoFunctionsYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after1 = before.AddFunctions();
        var after2 = before.AddFunctions(null!);
        Assert.Same(before, after1);
        Assert.Same(before, after2);
    }

    [Fact]
    public void Catalog_ReplacingWithSameFunctionsYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after = before.WithFunctions(before.Functions);
        Assert.Same(before, after);
    }

    [Fact]
    public void Catalog_AddingNoAggregatesYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after1 = before.AddAggregates();
        var after2 = before.AddAggregates(null!);
        Assert.Same(before, after1);
        Assert.Same(before, after2);
    }

    [Fact]
    public void Catalog_ReplacingWithSameAggregatesYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after = before.WithAggregates(before.Aggregates);
        Assert.Same(before, after);
    }

    [Fact]
    public void Catalog_AddingNoVariablesYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after1 = before.AddVariables();
        var after2 = before.AddVariables(null!);
        Assert.Same(before, after1);
        Assert.Same(before, after2);
    }

    [Fact]
    public void Catalog_ReplacingWithSameVariablesYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after = before.WithVariables(before.Variables);
        Assert.Same(before, after);
    }

    [Fact]
    public void Catalog_ReplacingWithSamePropertyProvidersYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after = before.WithPropertyProviders(before.PropertyProviders);
        Assert.Same(before, after);
    }

    [Fact]
    public void Catalog_ReplacingWithSameMethodProvidersYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after = before.WithMethodProviders(before.MethodProviders);
        Assert.Same(before, after);
    }

    [Fact]
    public void Catalog_ReplacingWithSameComparersYieldsSameInstance()
    {
        var before = Catalog.Empty;
        var after = before.WithComparers(before.Comparers);
        Assert.Same(before, after);
    }
}
