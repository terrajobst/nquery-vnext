using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Symbols;

namespace NQuery.Authoring.Tests.Completion.Providers;

public class FunctionSymbolCompletionProviderTests : SymbolCompletionProviderTests
{
    private static void AssertIsMatch(string query, string functionName)
    {
        var completionModel = GetCompletionModel(query);
        var catalog = completionModel.SemanticModel.Compilation.Catalog;

        var definition = catalog.Functions.Single(t => t.Name == functionName);
        var functionItem = completionModel.Items.Single(i => i.InsertionText == definition.Name);
        var function = Assert.IsType<FunctionSymbol>(functionItem.Symbol);
        var functionMarkup = SymbolMarkup.ForSymbol(function);

        Assert.Equal(Glyph.Function, functionItem.Glyph);
        Assert.Equal(definition.Name, functionItem.DisplayText);
        Assert.Equal(functionMarkup.ToString(), functionItem.Description);
        Assert.Same(definition, function.Definition);
    }

    [Fact]
    public void SymbolCompletionProvider_ReturnsFunctions_InGlobalContext()
    {
        var query = """
            SELECT  |
            FROM    Employees
            """;

        AssertIsMatch(query, "SIN");
    }

    [Fact]
    public void SymbolCompletionProvider_ReturnsFunctions_AfterText()
    {
        var query = """
            SELECT  S|
            FROM    Employees
            """;

        AssertIsMatch(query, "SIN");
    }

    [Fact]
    public void SymbolCompletionProvider_ReturnsFunctions_AfterContextualKeyword()
    {
        var query = """
            SELECT  Left|
            FROM    Employees
            """;

        AssertIsMatch(query, "LEFT");
    }
}
