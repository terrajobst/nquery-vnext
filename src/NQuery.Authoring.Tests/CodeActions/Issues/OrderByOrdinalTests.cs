using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Issues;

namespace NQuery.Authoring.Tests.CodeActions.Issues
{
    public class OrderByOrdinalTests : CodeIssueTests
    {
        protected override ICodeIssueProvider CreateProvider()
        {
            return new OrderByOrdinalCodeIssueProvider();
        }

        [Fact]
        public void OrderByOrdinal_DoesNotTrigger_ForNamedReference()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.FirstName + ' ' + e.LastName AS FullName
                FROM    Employees e
                ORDER   BY FullName
             ";

            var issues = GetIssues(query);
            Assert.Empty(issues);
        }

        [Fact]
        public void OrderByOrdinal_DoesNotTrigger_ForUnnamed()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.FirstName + ' ' + e.LastName
                FROM    Employees e
                ORDER   BY 2
             ";

            var issues = GetIssues(query);
            Assert.Empty(issues);
        }

        [Fact]
        public void OrderByOrdinal_FindsOrdinalReference()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.FirstName + ' ' + e.LastName AS FullName
                FROM    Employees e
                ORDER   BY 2
            ";

            var issues = GetIssues(query);
            Assert.Single(issues);
            Assert.Equal(CodeIssueKind.Warning, issues[0].Kind);
            Assert.Equal("2", query.Substring(issues[0].Span));
        }

        [Fact]
        public void OrderByOrdinal_FixesOrdinalReference()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.FirstName + ' ' + e.LastName AS FullName
                FROM    Employees e
                ORDER   BY 2
            ";

            var fixedQuery = @"
                SELECT  e.EmployeeID,
                        e.FirstName + ' ' + e.LastName AS FullName
                FROM    Employees e
                ORDER   BY FullName
            ";

            var issues = GetIssues(query);
            Assert.Single(issues);

            var action = issues.First().Actions.First();
            Assert.Equal("Replace ordinal by named column reference", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.Text.GetText());
        }
    }
}