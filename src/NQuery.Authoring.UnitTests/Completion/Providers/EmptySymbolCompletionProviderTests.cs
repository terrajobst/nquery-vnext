using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Authoring.UnitTests.Completion.Providers
{
    [TestClass]
    public class EmptySymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsEmpty(string query)
        {
            var completionModel = GetCompletionModel(query);
            Assert.AreEqual(0, completionModel.Items.Count);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsNothing_InMultiLineComment()
        {
            var query = @"
                /*
                 * Some e|
                 */
                SELECT  *
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsNothing_InSingleLineComment()
        {
            var query = @"
                // Some e|
                SELECT  *
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsNothing_InString()
        {
            var query = @"
                SELECT  'e|'
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsNothing_InUnresolvedTable()
        {
            var query = @"
                SELECT  e.|
                FROM    Xxx e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsNothing_InUntypedExpression()
        {
            var query = @"
                SELECT  (e.FirstName * e.LastName).|
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsNothing_AfterColumnAs()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName AS |
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsNothing_InColumnAlias()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName F|
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsNothing_AfterTableAs()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName
                FROM    Employees AS |
            ";

            AssertIsEmpty(query);
        }

        [TestMethod]
        public void SymbolCompletionProvider_ReturnsNothing_InTableAlias()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName
                FROM    Employees e|
            ";

            AssertIsEmpty(query);
        }
    }
}