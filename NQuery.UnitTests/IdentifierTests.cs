using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests
{
    [TestClass]
    public class IdentifierTests
    {
        [TestMethod]
        public void Identifiers_MatchingPlainIdentifiersIgnoresCase()
        {
            var token = Helpers.LexSingleToken("Test");

            Assert.IsFalse(token.IsQuotedIdentifier());
            Assert.IsFalse(token.IsQuotedIdentifier());
            Assert.IsTrue(token.IsTerminated());
            Assert.IsTrue(token.Matches("Test"));
            Assert.IsTrue(token.Matches("TEST"));
        }

        [TestMethod]
        public void Identifiers_MatchingParenthesizedIdentifiersIgnoresCase()
        {
            var token = Helpers.LexSingleToken("[Test 123]");

            Assert.IsTrue(token.IsParenthesizedIdentifier());
            Assert.IsFalse(token.IsQuotedIdentifier());
            Assert.IsTrue(token.IsTerminated());
            Assert.IsTrue(token.Matches("Test 123"));
            Assert.IsTrue(token.Matches("TEST 123"));
        }

        [TestMethod]
        public void Identifiers_MatchingParenthesizedIdentifiersIgnoresCase_EvenIfNotTerminated()
        {
            var token = Helpers.LexSingleToken("[Test 123");

            Assert.IsTrue(token.IsParenthesizedIdentifier());
            Assert.IsFalse(token.IsQuotedIdentifier());
            Assert.IsFalse(token.IsTerminated());
            Assert.IsTrue(token.Matches("Test 123"));
            Assert.IsTrue(token.Matches("TEST 123"));
        }

        [TestMethod]
        public void Identifiers_MatchingQuotedIdentifiersRespectsCase()
        {
            var token = Helpers.LexSingleToken("\"Test 123\"");

            Assert.IsFalse(token.IsParenthesizedIdentifier());
            Assert.IsTrue(token.IsQuotedIdentifier());
            Assert.IsTrue(token.IsTerminated());
            Assert.IsTrue(token.Matches("Test 123"));
            Assert.IsFalse(token.Matches("TEST 123"));
        }

        [TestMethod]
        public void Identifiers_MatchingQuotedIdentifiersRespectsCase_EvenIfNotTerminated()
        {
            var token = Helpers.LexSingleToken("\"Test 123");

            Assert.IsFalse(token.IsParenthesizedIdentifier());
            Assert.IsTrue(token.IsQuotedIdentifier());
            Assert.IsFalse(token.IsTerminated());
            Assert.IsTrue(token.Matches("Test 123"));
            Assert.IsFalse(token.Matches("TEST 123"));
        }
    }
}