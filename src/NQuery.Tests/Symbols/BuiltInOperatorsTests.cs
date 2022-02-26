﻿using Xunit;

namespace NQuery.Tests.Symbols
{
    public class BuiltInOperatorsTests : BuiltInSymbolsTests
    {
        [Fact]
        public void BuiltInOperators_Like()
        {
            AssertEvaluatesTo("'A' LIKE 'a'", true);
            AssertEvaluatesTo("'ab' LIKE 'a'", false);
            AssertEvaluatesTo("'ab' LIKE 'b'", false);
            AssertEvaluatesTo("'abc' LIKE '_b_'", true);
            AssertEvaluatesTo("'abcde' LIKE '%c%'", true);
            AssertEvaluatesTo("'2.10.1' LIKE '2.1.%'", false);
        }
    }
}
