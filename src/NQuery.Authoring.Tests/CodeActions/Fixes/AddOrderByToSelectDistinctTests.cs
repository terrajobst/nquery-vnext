using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Fixes;

namespace NQuery.Authoring.Tests.CodeActions.Fixes
{
    public class AddOrderByToSelectDistinctTests : CodeFixTests
    {
        protected override ICodeFixProvider CreateProvider()
        {
            return new AddOrderByToSelectDistinctCodeFixProvider();
        }

        [Fact]
        public void AddOrderByToSelectDistinct_InsertsExpression()
        {
            var query = @"
                SELECT  DISTINCT
                        e.FirstName,
                        e.LastName
                FROM    Employees e
                ORDER   BY e.BirthDate|
            ";

            var fixedQuery = @"
                SELECT  DISTINCT
                        e.FirstName,
                        e.LastName, e.BirthDate
                FROM    Employees e
                ORDER   BY e.BirthDate
            ";

            AssertFixes(query, fixedQuery, "Add e.BirthDate to SELECT list");
        }

        [Fact]
        public void AddOrderByToSelectDistinct_InsertReusesTrailingComma()
        {
            var query = @"
                SELECT  DISTINCT
                        e.FirstName,
                        e.LastName,
                FROM    Employees e
                ORDER   BY e.BirthDate|
            ";

            var fixedQuery = @"
                SELECT  DISTINCT
                        e.FirstName,
                        e.LastName, e.BirthDate
                FROM    Employees e
                ORDER   BY e.BirthDate
            ";

            AssertFixes(query, fixedQuery, "Add e.BirthDate to SELECT list");
        }

        [Fact]
        public void AddOrderByToSelectDistinct_InsertCommaAfterIncompleteExpression()
        {
            var query = @"
                SELECT  DISTINCT
                        e.FirstName,
                        e.LastName.Substring(1
                FROM    Employees e
                ORDER   BY e.BirthDate|
            ";

            var fixedQuery = @"
                SELECT  DISTINCT
                        e.FirstName,
                        e.LastName.Substring(1, e.BirthDate
                FROM    Employees e
                ORDER   BY e.BirthDate
            ";

            AssertFixes(query, fixedQuery, "Add e.BirthDate to SELECT list");
        }
    }
}