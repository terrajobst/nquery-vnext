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

        var compiledExpression = compiledQuery.CompileExpression();
        var value = compiledExpression.Evaluate();

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

    [Fact]
    public void CompiledQuery_CompileExpressionOfT_EvaluatesTyped()
    {
        var compiledQuery = Compile("1 + 2");

        var compiled = compiledQuery.CompileExpression<int>();

        Assert.Equal(typeof(int), compiled.Type);
        Assert.Equal(3, compiled.Evaluate());
    }

    [Fact]
    public void CompiledQuery_CompileExpressionOfT_SubstitutesWhenNull_ForSqlNull()
    {
        var compiledQuery = Compile("CAST(NULL AS int)");

        var compiled = compiledQuery.CompileExpression(-1);

        Assert.Equal(-1, compiled.Evaluate());
    }

    [Fact]
    public void CompiledQuery_CompileExpressionOfT_ReturnsWhenNull_ForEmptyQuery()
    {
        var syntaxTree = SyntaxTree.ParseQuery(string.Empty);
        var compiledQuery = Compilation.Create(Catalog.Empty, syntaxTree).Compile();

        Assert.Equal(0, compiledQuery.CompileExpression<int>().Evaluate());
        Assert.Equal(-1, compiledQuery.CompileExpression(-1).Evaluate());
    }

    private static CompiledQuery Compile(string expression)
    {
        var syntaxTree = SyntaxTree.ParseExpression(expression);
        return Compilation.Create(Catalog.Empty, syntaxTree).Compile();
    }
}
