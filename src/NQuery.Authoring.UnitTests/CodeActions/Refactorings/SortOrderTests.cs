using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    [TestClass]
    public class SortOrderTests : RefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new SortOrderCodeRefactoringProvider();
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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