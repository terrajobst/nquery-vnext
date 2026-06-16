using NQuery.CodeAnalysis;

namespace NQuery.Tests;

public class CompiledQueryTests
{
    [Fact]
    public void CompiledQuery_Expression_ReturnsNull_ForEmptyQuery()
    {
        var catalog = Catalog.Empty;
        var syntaxTree = SyntaxTree.ParseQuery(string.Empty);
        var compilation = Compilation.Create(catalog, syntaxTree);
        var compiledQuery = compilation.Compile();

        var expressionEvaluator = compiledQuery.CreateExpressionEvaluator();
        var value = expressionEvaluator.Evaluate();

        Assert.Null(value);
    }

    [Fact]
    public void CompiledQuery_Query_ReturnsNoResults_ForEmptyQuery()
    {
        var catalog = Catalog.Empty;
        var syntaxTree = SyntaxTree.ParseQuery(string.Empty);
        var compilation = Compilation.Create(catalog, syntaxTree);
        var compiledQuery = compilation.Compile();

        var reader = compiledQuery.CreateReader();
        Assert.Equal(0, reader.ColumnCount);
        Assert.False(reader.Read());
    }
}
