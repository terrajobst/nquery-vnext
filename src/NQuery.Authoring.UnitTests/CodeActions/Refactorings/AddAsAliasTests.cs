using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    [TestClass]
    public class AddAsAliasTests : RefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new AddAsAliasCodeRefactoringProvider();
        }

        [TestMethod]
        public void AddAsAlias_DoesNotTrigger_WhenKeywordIsAlreadyPresent()
        {
            var query = @"
                SELECT  e.EmployeeID
                FROM    Employees AS e|
            ";

            AssertDoesNotTrigger(query);
        }

        [TestMethod]
        public void AddAsAlias_InsertsAs()
        {
            var query = @"
                SELECT  e.EmployeeID
                FROM    Employees /* before */ e| /* after */
            ";

            var fixedQuery = @"
                SELECT  e.EmployeeID
                FROM    Employees /* before */ AS e /* after */
            ";

            AssertFixes(query, fixedQuery, "Add 'AS' keyword");
        }
    }
}