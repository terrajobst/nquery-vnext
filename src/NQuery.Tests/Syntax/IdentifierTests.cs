namespace NQuery.Tests.Syntax
{
    public class IdentifierTests
    {
        [Fact]
        public void Identifiers_MatchingPlainIdentifiersIgnoresCase()
        {
            var token = SyntaxFacts.ParseToken("Test");

            Assert.False(token.IsParenthesizedIdentifier());
            Assert.False(token.IsQuotedIdentifier());
            Assert.True(token.IsTerminated());
            Assert.True(token.Matches("Test"));
            Assert.True(token.Matches("TEST"));
        }

        [Fact]
        public void Identifiers_MatchingParenthesizedIdentifiersIgnoresCase()
        {
            var token = SyntaxFacts.ParseToken("[Test 123]");

            Assert.True(token.IsParenthesizedIdentifier());
            Assert.False(token.IsQuotedIdentifier());
            Assert.True(token.IsTerminated());
            Assert.True(token.Matches("Test 123"));
            Assert.True(token.Matches("TEST 123"));
        }

        [Fact]
        public void Identifiers_MatchingParenthesizedIdentifiersIgnoresCase_EvenIfNotTerminated()
        {
            var token = SyntaxFacts.ParseToken("[Test 123");

            Assert.True(token.IsParenthesizedIdentifier());
            Assert.False(token.IsQuotedIdentifier());
            Assert.False(token.IsTerminated());
            Assert.True(token.Matches("Test 123"));
            Assert.True(token.Matches("TEST 123"));
        }

        [Fact]
        public void Identifiers_MatchingQuotedIdentifiersRespectsCase()
        {
            var token = SyntaxFacts.ParseToken("\"Test 123\"");

            Assert.False(token.IsParenthesizedIdentifier());
            Assert.True(token.IsQuotedIdentifier());
            Assert.True(token.IsTerminated());
            Assert.True(token.Matches("Test 123"));
            Assert.False(token.Matches("TEST 123"));
        }

        [Fact]
        public void Identifiers_MatchingQuotedIdentifiersRespectsCase_EvenIfNotTerminated()
        {
            var token = SyntaxFacts.ParseToken("\"Test 123");

            Assert.False(token.IsParenthesizedIdentifier());
            Assert.True(token.IsQuotedIdentifier());
            Assert.False(token.IsTerminated());
            Assert.True(token.Matches("Test 123"));
            Assert.False(token.Matches("TEST 123"));
        }
    }
}