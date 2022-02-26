using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Refactorings
{
    public class QualifyColumnTests : CodeRefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new QualifyColumnCodeRefactoringProvider();
        }

        [Fact]
        public void QualifyColumn_DoesNotTrigger_WhenColumnIsQualified()
        {
            var query = @"
                SELECT  e.EmployeeID|
                FROM    Employees e
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void QualifyColumn_DoesNotTrigger_WhenColumnCannotBeResolved()
        {
            var query = @"
                SELECT  NotFirstName|
                FROM    Employees e
            ";

            AssertDoesNotTrigger(query);
        }

        [Fact]
        public void QualifyColumn_InsertsQualifier()
        {
            var query = @"
                SELECT  EmployeeID|
                FROM    Employees e
            ";

            var fixedQuery = @"
                SELECT  e.EmployeeID
                FROM    Employees e
            ";

            AssertFixes(query, fixedQuery, "Qualify column");
        }

        [Fact]
        public void QualifyColumn_InsertsQualifierAndEscapes()
        {
            var query = @"
                SELECT  EmployeeID|
                FROM    Employees [the ]]emp]
            ";

            var fixedQuery = @"
                SELECT  [the ]]emp].EmployeeID
                FROM    Employees [the ]]emp]
            ";

            AssertFixes(query, fixedQuery, "Qualify column");
        }
    }
}
