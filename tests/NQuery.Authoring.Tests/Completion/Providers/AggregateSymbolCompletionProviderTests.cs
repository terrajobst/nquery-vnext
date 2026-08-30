using NQuery.CodeAnalysis.Symbols;

namespace NQuery.Authoring.Tests.Completion.Providers;

public class AggregateSymbolCompletionProviderTests : SymbolCompletionProviderTests
{
    private static void AssertIsMatch(string query, string aggregateName)
    {
        var completionResult = GetCompletionResult(query);
        var catalog = completionResult.SemanticModel.Compilation.Catalog;

        var definition = catalog.Aggregates.Single(t => t.Name == aggregateName);
        var functionItem = completionResult.Items.Single(i => i.InsertionText == definition.Name);
        var function = Assert.IsType<AggregateSymbol>(functionItem.Symbol);
        var functionMarkup = SymbolMarkup.ForSymbol(function);

        Assert.Equal(Glyph.Aggregate, functionItem.Glyph);
        Assert.Equal(definition.Name, functionItem.DisplayText);
        Assert.Equal(functionMarkup.ToString(), functionItem.Description);
        Assert.Same(definition, function.Definition);
    }

    [Fact]
    public void SymbolCompletionProvider_ReturnsAggregates_InGlobalContext()
    {
        var query = """
            SELECT  |
            FROM    Employees
            """;

        AssertIsMatch(query, "COUNT");
    }

    [Fact]
    public void SymbolCompletionProvider_ReturnsAggregates_AfterText()
    {
        var query = """
            SELECT  S|
            FROM    Employees
            """;

        AssertIsMatch(query, "SUM");
    }
}
