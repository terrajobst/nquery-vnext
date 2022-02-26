using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Refactorings
{
    public class AddMissingKeywordTests : CodeRefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new AddMissingKeywordCodeRefactoringProvider();
        }

        [Fact]
        public void AddMissingKeyword_DetectedForOrderWithoutBy()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   /* before */ |e.FirstName
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   /* before */ BY e.FirstName
            ";

            var description = "Add missing 'BY' keyword";

            AssertFixes(query, fixedQuery, description);
        }
    }
}