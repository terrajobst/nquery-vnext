using System;
using System.Linq;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Issues;

using Xunit;

namespace NQuery.Authoring.Tests.CodeActions.Issues
{
    public class UnusedCommonTableExpressionTests : CodeIssueTests
    {
        protected override ICodeIssueProvider CreateProvider()
        {
            return new UnusedCommonTableExpressionCodeIssueProvider();
        }

        [Fact]
        public void UnusedCommonTableExpression_FindsUnusedNonRecursive()
        {
            var query = @"
                WITH Emps AS (
                    SELECT  *
                    FROM    Employees e
                ), Emps2 AS (
                    SELECT  *
                    FROM    Employees e
                )
                SELECT  *
                FROM    Emps e
            ";

            var unusedCte = @"Emps2 AS (
                    SELECT  *
                    FROM    Employees e
                )";

            var issues = GetIssues(query);

            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Unnecessary, issues[0].Kind);
            Assert.Equal(unusedCte, query.Substring(issues[0].Span));
        }

        [Fact]
        public void UnusedCommonTableExpression_FindsUnuseRecursive()
        {
            var query = @"
                WITH Emps AS (
                    SELECT  *
                    FROM    Employees e
                    WHERE   e.ReportsTo IS NULL
                    UNION   ALL
                    SELECT  *
                    FROM    Employees e
                                INNER JOIN Emps m ON m.EmployeeID = e.ReportsTo
                ), EmpsUnused AS (
                    SELECT  *
                    FROM    Employees e
                    WHERE   e.ReportsTo IS NULL
                    UNION   ALL
                    SELECT  *
                    FROM    Employees e
                                INNER JOIN EmpsUnused m ON m.EmployeeID = e.ReportsTo
                )
                SELECT  *
                FROM    Emps e
            ";

            var unusedCte = @"EmpsUnused AS (
                    SELECT  *
                    FROM    Employees e
                    WHERE   e.ReportsTo IS NULL
                    UNION   ALL
                    SELECT  *
                    FROM    Employees e
                                INNER JOIN EmpsUnused m ON m.EmployeeID = e.ReportsTo
                )";

            var issues = GetIssues(query);

            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Unnecessary, issues[0].Kind);
            Assert.Equal(unusedCte, query.Substring(issues[0].Span));
        }

        [Fact]
        public void UnusedCommonTableExpression_FixedUnusedNonRecursive_IfSingle()
        {
            var query = @"
                WITH EmpsUnused AS (
                    SELECT  *
                    FROM    Employees e
                )
                SELECT  *
                FROM    Employees
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees
            ";

            var issues = GetIssues(query);

            var action = issues.Single().Actions.Single();
            Assert.Equal("Remove unused common table expression", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.Text.GetText());
        }

        [Fact]
        public void UnusedCommonTableExpression_FixedUnusedNonRecursive_IfFirst()
        {
            var query = @"
                WITH EmpsUnused AS (
                    SELECT  *
                    FROM    Employees e
                ), Emps1 AS (
                    SELECT  *
                    FROM    Employees e
                ), Emps2 AS (
                    SELECT  *
                    FROM    Employees e
                )
                SELECT  *
                FROM    Emps1 e1
                            INNER JOIN Emps2 e2 ON e2.EmployeeID = e1.ReportsTo
            ";

            var fixedQuery = @"
                WITH Emps1 AS (
                    SELECT  *
                    FROM    Employees e
                ), Emps2 AS (
                    SELECT  *
                    FROM    Employees e
                )
                SELECT  *
                FROM    Emps1 e1
                            INNER JOIN Emps2 e2 ON e2.EmployeeID = e1.ReportsTo
            ";

            var issues = GetIssues(query);

            var action = issues.Single().Actions.Single();
            Assert.Equal("Remove unused common table expression", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.Text.GetText());
        }

        [Fact]
        public void UnusedCommonTableExpression_FixedUnusedNonRecursive_IfMiddle()
        {
            var query = @"
                WITH Emps1 AS (
                    SELECT  *
                    FROM    Employees e
                ), EmpsUnused AS (
                    SELECT  *
                    FROM    Employees e
                ), Emps2 AS (
                    SELECT  *
                    FROM    Employees e
                )
                SELECT  *
                FROM    Emps1 e1
                            INNER JOIN Emps2 e2 ON e2.EmployeeID = e1.ReportsTo
            ";

            var fixedQuery = @"
                WITH Emps1 AS (
                    SELECT  *
                    FROM    Employees e
                ), Emps2 AS (
                    SELECT  *
                    FROM    Employees e
                )
                SELECT  *
                FROM    Emps1 e1
                            INNER JOIN Emps2 e2 ON e2.EmployeeID = e1.ReportsTo
            ";

            var issues = GetIssues(query);

            var action = issues.Single().Actions.Single();
            Assert.Equal("Remove unused common table expression", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.Text.GetText());
        }

        [Fact]
        public void UnusedCommonTableExpression_FixedUnusedNonRecursive_IfLast()
        {
            var query = @"
                WITH Emps1 AS (
                    SELECT  *
                    FROM    Employees e
                ), Emps2 AS (
                    SELECT  *
                    FROM    Employees e
                ), EmpsUnused AS (
                    SELECT  *
                    FROM    Employees e
                )
                SELECT  *
                FROM    Emps1 e1
                            INNER JOIN Emps2 e2 ON e2.EmployeeID = e1.ReportsTo
            ";

            var fixedQuery = @"
                WITH Emps1 AS (
                    SELECT  *
                    FROM    Employees e
                ), Emps2 AS (
                    SELECT  *
                    FROM    Employees e
                )
                SELECT  *
                FROM    Emps1 e1
                            INNER JOIN Emps2 e2 ON e2.EmployeeID = e1.ReportsTo
            ";

            var issues = GetIssues(query);

            var action = issues.Single().Actions.Single();
            Assert.Equal("Remove unused common table expression", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.Text.GetText());
        }
    }
}