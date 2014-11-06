using System.Linq;

using Xunit;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Issues;

namespace NQuery.Authoring.UnitTests.CodeActions.Issues
{
    public class ComparisonWithNullTests : CodeIssueTests
    {
        protected override ICodeIssueProvider CreateProvider()
        {
            return new ComparisonWithNullCodeIssueProvider();
        }

        [Fact]
        public void ComparisonWithNull_FindsUsageOfExpressionThatYieldsNull()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo + NULL > 1
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Warning, issues[0].Kind);
            Assert.Equal("Expression is always NULL", issues[0].Description);
            Assert.Equal("e.ReportsTo + NULL", query.Substring(issues[0].Span));
        }

        [Fact]
        public void ComparisonWithNull_FindsUsageOfEqualsNull()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo = NULL
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Warning, issues[0].Kind);
            Assert.Equal("e.ReportsTo = NULL", query.Substring(issues[0].Span));
        }

        [Fact]
        public void ComparisonWithNull_FixesUsageOfEqualsNull()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo = NULL
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo IS NULL
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.Equal("Convert to IS NULL", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [Fact]
        public void ComparisonWithNull_FindsUsageOfNotEqualsNull1()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo != NULL
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Warning, issues[0].Kind);
            Assert.Equal("e.ReportsTo != NULL", query.Substring(issues[0].Span));
        }

        [Fact]
        public void ComparisonWithNull_FixesUsageOfNotEqualsNull1()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo != NULL
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.Equal("Convert to IS NOT NULL", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [Fact]
        public void ComparisonWithNull_FindsUsageOfNotEqualsNull2()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo <> NULL
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Warning, issues[0].Kind);
            Assert.Equal("e.ReportsTo <> NULL", query.Substring(issues[0].Span));
        }

        [Fact]
        public void ComparisonWithNull_FixesUsageOfNotEqualsNull2()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo <> NULL
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.Equal("Convert to IS NOT NULL", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [Fact]
        public void ComparisonWithNull_FindsUsageOfEqualsNull_Reversed()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   NULL = e.ReportsTo
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Warning, issues[0].Kind);
            Assert.Equal("NULL = e.ReportsTo", query.Substring(issues[0].Span));
        }

        [Fact]
        public void ComparisonWithNull_FixesUsageOfEqualsNull_Reversed()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   NULL = e.ReportsTo
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo IS NULL
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.Equal("Convert to IS NULL", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [Fact]
        public void ComparisonWithNull_FindsUsageOfNotEqualsNull1_Reversed()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   NULL != e.ReportsTo 
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Warning, issues[0].Kind);
            Assert.Equal("NULL != e.ReportsTo", query.Substring(issues[0].Span));
        }

        [Fact]
        public void ComparisonWithNull_FixesUsageOfNotEqualsNull1_Reversed()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   NULL != e.ReportsTo
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.Equal("Convert to IS NOT NULL", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [Fact]
        public void ComparisonWithNull_FindsUsageOfNotEqualsNull2_Reversed()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   NULL <> e.ReportsTo
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);
            Assert.Equal(CodeIssueKind.Warning, issues[0].Kind);
            Assert.Equal("NULL <> e.ReportsTo", query.Substring(issues[0].Span));
        }

        [Fact]
        public void ComparisonWithNull_FixesUsageOfNotEqualsNull2_Reversed()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   NULL <> e.ReportsTo
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   e.ReportsTo IS NOT NULL
            ";

            var issues = GetIssues(query);
            Assert.Equal(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.Equal("Convert to IS NOT NULL", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.Equal(fixedQuery, syntaxTree.TextBuffer.GetText());
        }
    }
}