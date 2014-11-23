using System;

using Xunit;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    public class AddAsAliasTests : CodeRefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new AddAsAliasCodeRefactoringProvider();
        }

        [Fact]
        public void AddAsAlias_DoesNotTrigger_WhenKeywordIsAlreadyPresent()
        {
            var query = @"
                SELECT  e.EmployeeID
                FROM    Employees AS e|
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
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