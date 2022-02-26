using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Refactorings
{
    public class SortOrderTests : CodeRefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new SortOrderCodeRefactoringProvider();
        }

        [Fact]
        public void SortOrder_CanConvertImplicitToExplicit()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName| /*after*/
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName ASC /*after*/
            ";

            var description = "To explicit sort order";

            AssertFixes(query, fixedQuery, description);
        }

        [Fact]
        public void SortOrder_CanConvertImplicitToDescending()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName| /*after*/
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName DESC /*after*/
            ";

            var description = "To descending";

            AssertFixes(query, fixedQuery, description);
        }

        [Fact]
        public void SortOrder_CanConvertExplicitToImplicit()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName ASC| /*after*/
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName /*after*/
            ";

            var description = "To implicit sort order";

            AssertFixes(query, fixedQuery, description);
        }

        [Fact]
        public void SortOrder_CanConvertExplicitToAscending()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName ASC| /*after*/
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName DESC /*after*/
            ";

            var description = "To descending";

            AssertFixes(query, fixedQuery, description);
        }
    }
}