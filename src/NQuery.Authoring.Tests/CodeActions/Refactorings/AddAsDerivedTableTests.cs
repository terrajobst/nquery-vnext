using System;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Refactorings
{
    public class AddAsDerivedTableTests : CodeRefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new AddAsDerivedTableCodeRefactoringProvider();
        }

        [Fact]
        public void AddAsAlias_DoesNotTrigger_WhenKeywordIsAlreadyPresent()
        {
            var query = @"
                SELECT  *
                FROM    (
                            SELECT  *
                            FROM    Employees e
                        ) AS d|
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void AddAsAlias_InsertsAs()
        {
            var query = @"
                SELECT  *
                FROM    (
                            SELECT  *
                            FROM    Employees e
                        ) /* before */ d| /* after */
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    (
                            SELECT  *
                            FROM    Employees e
                        ) /* before */ AS d /* after */
            ";

            AssertFixes(query, fixedQuery, "Add 'AS' keyword");
        }
    
    }
}