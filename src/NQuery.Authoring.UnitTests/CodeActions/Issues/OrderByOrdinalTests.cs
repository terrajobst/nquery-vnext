using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Issues;

namespace NQuery.Authoring.UnitTests.CodeActions.Issues
{
    [TestClass]
    public class OrderByOrdinalTests : CodeIssueTests
    {
        protected override ICodeIssueProvider CreateProvider()
        {
            return new OrderByOrdinalCodeIssueProvider();
        }

        [TestMethod]
        public void OrderByOrdinal_DoesNotTrigger_ForNamedReference()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.FirstName + ' ' + e.LastName AS FullName
                FROM    Employees e
                ORDER   BY FullName
             ";

            var issues = GetIssues(query);
            Assert.AreEqual(0, issues.Length);
        }

        [TestMethod]
        public void OrderByOrdinal_DoesNotTrigger_ForUnnamed()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.FirstName + ' ' + e.LastName
                FROM    Employees e
                ORDER   BY 2
             ";

            var issues = GetIssues(query);
            Assert.AreEqual(0, issues.Length);
        }

        [TestMethod]
        public void OrderByOrdinal_FindsOrdinalReference()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.FirstName + ' ' + e.LastName AS FullName
                FROM    Employees e
                ORDER   BY 2
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual(CodeIssueKind.Warning, issues[0].Kind);
            Assert.AreEqual("2", query.Substring(issues[0].Span));
        }

        [TestMethod]
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
            Assert.AreEqual(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.AreEqual("Replace ordinal by named column reference", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }
    }
}