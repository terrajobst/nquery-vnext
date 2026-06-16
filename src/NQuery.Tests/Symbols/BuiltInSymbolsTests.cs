namespace NQuery.Tests.Symbols;

public class BuiltInSymbolsTests
{
    protected static void AssertEvaluatesTo(string text, object expectedValue)
    {
        var actualValue = Compute(text);

        Assert.Equal(expectedValue, actualValue);
    }

    protected static object Compute(string text)
    {
        var catalog = Catalog.Default;
        var expression = Expression<object>.Create(catalog, text);
        return expression.Evaluate();
    }
}
