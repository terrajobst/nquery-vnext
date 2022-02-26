using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.Tests.CodeActions.Refactorings
{
    public class RemoveRedundantBracketsTests : CodeRefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new RemoveRedundantBracketsCodeRefactoringProvider();
        }

        [Fact]
        public void RemoveRedundantBrackets_DoesNotTrigger_WhenBracketsAreRequired()
        {
            var query = @"
                SELECT  COUNT(*) [#Rows|]
                FROM    Employees
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void RemoveRedundantBrackets_DoesNotTrigger_ForKeywords()
        {
            var query = @"
                SELECT  COUNT(*) [#Rows]
                FROM|    Employees
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void RemoveRedundantBrackets_RemovesBrackets()
        {
            var query = @"
                SELECT  COUNT(*) [Rows|]
                FROM    Employees
            ";

            var fixedQuery = @"
                SELECT  COUNT(*) Rows
                FROM    Employees
            ";

            AssertFixes(query, fixedQuery, "Remove redundant brackets");
        }
    }
}