using System;

using Xunit;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    public class ExpandWildcardTests : CodeRefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new ExpandWildcardCodeRefactoringProvider();
        }

        [Fact]
        public void ExpandWildcard_DoesNotTriggerForUnresolvedTable()
        {
            var query = @"
                SELECT  *|
                FROM    Xxx
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void ExpandWildcard_SimpleStarExpandsAllTables()
        {
            var query = @"
                SELECT  /* before */ *| /* after */
                FROM    Employees e,
                        EmployeeTerritories et
            ";

            var fixedQuery = @"
                SELECT  /* before */ e.EmployeeID,
                                     e.LastName,
                                     e.FirstName,
                                     e.Title,
                                     e.TitleOfCourtesy,
                                     e.BirthDate,
                                     e.HireDate,
                                     e.Address,
                                     e.City,
                                     e.Region,
                                     e.PostalCode,
                                     e.Country,
                                     e.HomePhone,
                                     e.Extension,
                                     e.Photo,
                                     e.Notes,
                                     e.ReportsTo,
                                     e.PhotoPath,
                                     et.EmployeeID,
                                     et.TerritoryID /* after */
                FROM    Employees e,
                        EmployeeTerritories et
            ";

            var description = "Expand wildcard";

            AssertFixes(query, fixedQuery, description);
        }

        [Fact]
        public void ExpandWildcard_QualifiedStarExpandsSingleTable()
        {
            var query = @"
                SELECT  /* before */ et.*| /* after */
                FROM    Employees e,
                        EmployeeTerritories et
            ";

            var fixedQuery = @"
                SELECT  /* before */ et.EmployeeID,
                                     et.TerritoryID /* after */
                FROM    Employees e,
                        EmployeeTerritories et
            ";

            var description = "Expand wildcard";

            AssertFixes(query, fixedQuery, description);
        }
    }
}