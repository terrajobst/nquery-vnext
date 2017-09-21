using System;

using Xunit;

namespace NQuery.Tests.Symbols
{
    public class BuiltInSymbolsTests
    {
        protected static void AssertEvaluatesTo(string text, object expectedValue)
        {
            var actualValue = Compute(text);

            Assert.Equal(expectedValue, actualValue);
        }

        protected static object Compute(string text)
        {
            var dataContext = DataContext.Default;
            var expression = Expression<object>.Create(dataContext, text);
            return expression.Evaluate();
        }
    }
}
