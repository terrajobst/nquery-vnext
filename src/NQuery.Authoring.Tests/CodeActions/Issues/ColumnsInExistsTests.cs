using System.Linq;

using Xunit;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Issues;

namespace NQuery.Authoring.UnitTests.CodeActions.Issues
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

            Assert.Equal(0, codeIssues.Length);
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
            Assert.Equal(1, codeIssues[2].Actions.Length);

            Assert.Equal(1, codeIssues[0].Actions.Length);
            Assert.Equal(1, codeIssues[1].Actions.Length);
            Assert.Equal(1, codeIssues[2].Actions.Length);
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
            Assert.Equal(1, codeIssues[0].Actions.Length);
            Assert.Equal(1, codeIssues[1].Actions.Length);
            Assert.Equal(1, codeIssues[2].Actions.Length);
            Assert.Same(codeIssues[0].Actions[0], codeIssues[1].Actions[0]);
            Assert.Same(codeIssues[1].Actions[0], codeIssues[2].Actions[0]);

            var action = codeIssues.First().Actions.First();
            Assert.Equal("Remove unnecessary columns from EXISTS", action.Description);

            var syntaxTree = action.GetEdit();
            
            Assert.Equal(fixedQuery, syntaxTree.TextBuffer.GetText());
        }
    }
}