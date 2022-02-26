using Xunit;

namespace NQuery.Authoring.Tests.Completion.Providers
{
    public class EmptySymbolCompletionProviderTests : SymbolCompletionProviderTests
    {
        private static void AssertIsEmpty(string query)
        {
            var completionModel = GetCompletionModel(query);
            Assert.Empty(completionModel.Items);
        }

        [Fact]
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

        [Fact]
        public void SymbolCompletionProvider_ReturnsNothing_InSingleLineComment()
        {
            var query = @"
                // Some e|
                SELECT  *
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsNothing_InString()
        {
            var query = @"
                SELECT  'e|'
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsNothing_InUnresolvedTable()
        {
            var query = @"
                SELECT  e.|
                FROM    Xxx e
            ";

            AssertIsEmpty(query);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsNothing_InUntypedExpression()
        {
            var query = @"
                SELECT  (e.FirstName * e.LastName).|
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsNothing_AfterColumnAs()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName AS |
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsNothing_InColumnAlias()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName F|
                FROM    Employees e
            ";

            AssertIsEmpty(query);
        }

        [Fact]
        public void SymbolCompletionProvider_ReturnsNothing_AfterTableAs()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName
                FROM    Employees AS |
            ";

            AssertIsEmpty(query);
        }

        [Fact]
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