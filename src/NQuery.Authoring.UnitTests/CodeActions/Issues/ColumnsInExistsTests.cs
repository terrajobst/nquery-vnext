using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Issues;

namespace NQuery.Authoring.UnitTests.CodeActions.Issues
{
    [TestClass]
    public class ColumnsInExistsTests
    {
        private static CodeIssue[] GetCodeIssues(string query)
        {
            var compilation = CompilationFactory.CreateQuery(query);
            var semanticModel = compilation.GetSemanticModel();

            var columnsInExistsCodeIssueProvider = new ColumnsInExistsCodeIssueProvider();
            return columnsInExistsCodeIssueProvider.GetIssues(semanticModel).ToArray();
        }

        [TestMethod]
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

            var codeIssues = GetCodeIssues(query);

            Assert.AreEqual(0, codeIssues.Length);
        }

        [TestMethod]
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

            var codeIssues = GetCodeIssues(query);

            Assert.AreEqual(3, codeIssues.Length);
            
            Assert.AreEqual(CodeIssueKind.Unnecessary, codeIssues[0].Kind);
            Assert.AreEqual("et.EmployeeID", query.Substring(codeIssues[0].Span));

            Assert.AreEqual(CodeIssueKind.Unnecessary, codeIssues[1].Kind);
            Assert.AreEqual(",", query.Substring(codeIssues[1].Span));

            Assert.AreEqual(CodeIssueKind.Unnecessary, codeIssues[2].Kind);
            Assert.AreEqual("et.TerritoryID", query.Substring(codeIssues[2].Span));
            Assert.AreEqual(1, codeIssues[2].Actions.Count);

            Assert.AreEqual(1, codeIssues[0].Actions.Count);
            Assert.AreEqual(1, codeIssues[1].Actions.Count);
            Assert.AreEqual(1, codeIssues[2].Actions.Count);
        }

        [TestMethod]
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

            var codeIssues = GetCodeIssues(query);

            Assert.AreEqual(3, codeIssues.Length);           
            Assert.AreEqual(1, codeIssues[0].Actions.Count);
            Assert.AreEqual(1, codeIssues[1].Actions.Count);
            Assert.AreEqual(1, codeIssues[2].Actions.Count);
            Assert.AreSame(codeIssues[0].Actions[0], codeIssues[1].Actions[0]);
            Assert.AreSame(codeIssues[1].Actions[0], codeIssues[2].Actions[0]);

            var action = codeIssues.First().Actions.First();
            Assert.AreEqual("Remove unnecessary columns from EXISTS", action.Description);

            var syntaxTree = action.GetEdit();
            
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }
    }
}