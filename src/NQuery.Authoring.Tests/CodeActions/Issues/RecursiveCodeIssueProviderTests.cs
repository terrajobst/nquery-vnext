using System;
using System.Linq;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Issues;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Issues
{
    public class RecursiveCodeIssueProviderTests : CodeIssueTests
    {
        protected override ICodeIssueProvider CreateProvider()
        {
            return new RecursiveCodeIssueProvider();
        }

        [Fact]
        public void RecursiveCodeIssueProvider_FixesMissingRecursive()
        {
            var query = @"
                WITH Emps AS
                (
                    SELECT  *
                    FROM    Employees

                    UNION   ALL

                    SELECT  *
                    FROM    Emps
                )
                SELECT  *
                FROM    Emps
            ";

            var fixedQuery = @"
                WITH RECURSIVE Emps AS
                (
                    SELECT  *
                    FROM    Employees

                    UNION   ALL

                    SELECT  *
                    FROM    Emps
                )
                SELECT  *
                FROM    Emps
            ";


            var issues = GetIssues(query);

            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Warning, issues[0].Kind);
            Assert.Equal("Emps", query.Substring(issues[0].Span));

            var action = issues.Single().Actions.Single();
            Assert.Equal("Add missing RECURSIVE keyword", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.Text.GetText());
        }

        [Fact]
        public void RecursiveCodeIssueProvider_FixesUnncessaryRecursive()
        {
            var query = @"
                WITH RECURSIVE Emps AS
                (
                    SELECT  *
                    FROM    Employees
                )
                SELECT  *
                FROM    Emps
            ";

            var fixedQuery = @"
                WITH Emps AS
                (
                    SELECT  *
                    FROM    Employees
                )
                SELECT  *
                FROM    Emps
            ";

            var issues = GetIssues(query);

            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Unnecessary, issues[0].Kind);
            Assert.Equal("RECURSIVE", query.Substring(issues[0].Span));

            var action = issues.Single().Actions.Single();
            Assert.Equal("Remove unnecessary RECURSIVE keyword", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.Text.GetText());
        }

        [Fact]
        public void RecursiveCodeIssueProvider_DoesNotTrigger_ForRecursive()
        {
            var query = @"
                WITH RECURSIVE Emps AS
                (
                    SELECT  *
                    FROM    Employees

                    UNION   ALL

                    SELECT  *
                    FROM    Emps
                )
                SELECT  *
                FROM    Emps
            ";

            var issues = GetIssues(query);
            Assert.Empty(issues);
        }

        [Fact]
        public void RecursiveCodeIssueProvider_DoesNotTrigger_ForNonRecursive()
        {
            var query = @"
                WITH Emps AS
                (
                    SELECT  *
                    FROM    Employees
                )
                SELECT  *
                FROM    Emps
            ";

            var issues = GetIssues(query);
            Assert.Empty(issues);
        }
    }
}