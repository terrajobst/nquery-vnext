using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Issues;

namespace NQuery.Authoring.Tests.CodeActions.Issues
{
    public class ColumnsInExistsTests : CodeIssueTests
    {
        protected override ICodeIssueProvider CreateProvider()
        {
            return new ColumnsInExistsCodeIssueProvider();
        }

        [Fact]
        public void ColumnInExists_DoesNotTrigger_ForSelectStar()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   EXISTS (
                            SELECT  *
                            FROM    EmployeeTerritories et
                            WHERE   et.EmployeeID = e.EmployeeID
                        )
            ";

            var codeIssues = GetIssues(query);

            Assert.Empty(codeIssues);
        }

        [Fact]
        public void ColumnInExists_FindsUnnecessaryColumns()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   EXISTS (
                            SELECT  -- Before
                                    et.EmployeeID, -- Test 1
                                    et.TerritoryID -- Test 2
                                    -- After
                            FROM    EmployeeTerritories et
                            WHERE   et.EmployeeID = e.EmployeeID
                        )
            ";

            var codeIssues = GetIssues(query);

            Assert.Equal(3, codeIssues.Length);

            Assert.Equal(CodeIssueKind.Unnecessary, codeIssues[0].Kind);
            Assert.Equal("et.EmployeeID", query.Substring(codeIssues[0].Span));

            Assert.Equal(CodeIssueKind.Unnecessary, codeIssues[1].Kind);
            Assert.Equal(",", query.Substring(codeIssues[1].Span));

            Assert.Equal(CodeIssueKind.Unnecessary, codeIssues[2].Kind);
            Assert.Equal("et.TerritoryID", query.Substring(codeIssues[2].Span));
            Assert.Single(codeIssues[2].Actions);

            Assert.Single(codeIssues[0].Actions);
            Assert.Single(codeIssues[1].Actions);
            Assert.Single(codeIssues[2].Actions);
        }

        [Fact]
        public void ColumnInExists_FixesUnnecessaryColumns()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                WHERE   EXISTS (
                            SELECT  -- Before
                                    et.EmployeeID, -- Test 1
                                    et.TerritoryID -- Test 2
                                    -- After
                            FROM    EmployeeTerritories et
                            WHERE   et.EmployeeID = e.EmployeeID
                        )
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                WHERE   EXISTS (
                            SELECT  -- Before
                                    * -- Test 2
                                    -- After
                            FROM    EmployeeTerritories et
                            WHERE   et.EmployeeID = e.EmployeeID
                        )
            ";

            var codeIssues = GetIssues(query);

            Assert.Equal(3, codeIssues.Length);
            Assert.Single(codeIssues[0].Actions);
            Assert.Single(codeIssues[1].Actions);
            Assert.Single(codeIssues[2].Actions);
            Assert.Same(codeIssues[0].Actions[0], codeIssues[1].Actions[0]);
            Assert.Same(codeIssues[1].Actions[0], codeIssues[2].Actions[0]);

            var action = codeIssues.First().Actions.First();
            Assert.Equal("Remove unnecessary columns from EXISTS", action.Description);

            var syntaxTree = action.GetEdit();

            Assert.Equal(fixedQuery, syntaxTree.Text.GetText());
        }
    }
}