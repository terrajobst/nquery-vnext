using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    [TestClass]
    public class ExpandWildcardTests : RefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new ExpandWildcardCodeRefactoringProvider();
        }

        [TestMethod]
        public void ExpandWildcard_DoesNotTriggerForUnresolvedTable()
        {
            var query = @"
                SELECT  *|
                FROM    Xxx
            ";

            AssertDoesNotTrigger(query);
        }

        [TestMethod]
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

        [TestMethod]
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