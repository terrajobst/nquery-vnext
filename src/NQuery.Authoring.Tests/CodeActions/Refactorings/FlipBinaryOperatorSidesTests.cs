using System;

using Xunit;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    public class FlipBinaryOperatorSidesTests : RefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new FlipBinaryOperatorSidesCodeRefactoringProvider();
        }

        [Fact]
        public void FlipBinaryOperatorSides_SwapsOperators()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   /* prefix */ e.FirstName /* before */ =| /* after */ 'Andrew' /* suffix */
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   /* prefix */ 'Andrew' /* before */ = /* after */ e.FirstName /* suffix */
            ";

            var description = "Flip arguments of operator '='";

            AssertFixes(query, fixedQuery, description);
        }
    }
}