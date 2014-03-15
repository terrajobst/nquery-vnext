using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Issues;

namespace NQuery.Authoring.UnitTests.CodeActions.Issues
{
    [TestClass]
    public class OrderByExpressionsTests
    {
        private static CodeIssue[] GetIssues(string query)
        {
            var compilation = CompilationFactory.CreateQuery(query);
            var semanticModel = compilation.GetSemanticModel();

            var provider = new OrderByExpressionsCodeIssueProvider();
            return provider.GetIssues(semanticModel).ToArray();
        }

        [TestMethod]
        public void OrderByExpressions_DoesNotTrigger_ForColumnExpressions()
        {
            var query = @"
                SELECT  FirstName,
                        e.LastName
                FROM    Employees e
                ORDER   BY FirstName, e.LastName
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(0, issues.Length);
        }

        [TestMethod]
        public void OrderByExpressions_DoesNotTrigger_ForOrdinalReferences()
        {
            var query = @"
                SELECT  FirstName + ' ' + e.LastName
                FROM    Employees e
                ORDER   BY 1
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(0, issues.Length);
        }

        [TestMethod]
        public void OrderByExpressions_FindsUsageOfExpressionThatCanBeReplacedWithOrdinal()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName
                FROM    Employees e
                ORDER   BY e.FirstName /* marker */ + ' ' + e.LastName
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual(CodeIssueKind.Warning, issues[0].Kind);
            Assert.AreEqual("e.FirstName /* marker */ + ' ' + e.LastName", query.Substring(issues[0].Span));
        }

        [TestMethod]
        public void OrderByExpressions_FixesUsageOfExpressionThatCanBeReplacedWithOrdinal()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName
                FROM    Employees e
                ORDER   BY e.FirstName /* marker */ + ' ' + e.LastName
            ";

            var fixedQuery = @"
                SELECT  e.FirstName + ' ' + e.LastName
                FROM    Employees e
                ORDER   BY 1
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.AreEqual("Replace expression by SELECT column reference", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [TestMethod]
        public void OrderByExpressions_FindsUsageOfExpressionThatCanBeReplacedWithAlias()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName AS FullName
                FROM    Employees e
                ORDER   BY e.FirstName /* marker */ + ' ' + e.LastName
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual(CodeIssueKind.Warning, issues[0].Kind);
            Assert.AreEqual("e.FirstName /* marker */ + ' ' + e.LastName", query.Substring(issues[0].Span));
        }

        [TestMethod]
        public void OrderByExpressions_FixesUsageOfExpressionThatCanBeReplacedWithAlias()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName AS FullName
                FROM    Employees e
                ORDER   BY e.FirstName /* marker */ + ' ' + e.LastName
            ";

            var fixedQuery = @"
                SELECT  e.FirstName + ' ' + e.LastName AS FullName
                FROM    Employees e
                ORDER   BY FullName
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.AreEqual("Replace expression by SELECT column reference", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [TestMethod]
        public void OrderByExpressions_FindsUsageOfExpressionThatCanBeReplacedWithOrdinal_IfAppliedToParenthesizedQuery()
        {
            var query = @"
                (
                    SELECT  e.FirstName + ' ' + e.LastName
                    FROM    Employees e
                )
                ORDER   BY e.FirstName /* marker */ + ' ' + e.LastName
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual(CodeIssueKind.Warning, issues[0].Kind);
            Assert.AreEqual("e.FirstName /* marker */ + ' ' + e.LastName", query.Substring(issues[0].Span));
        }

        [TestMethod]
        public void OrderByExpressions_FixesUsageOfExpressionThatCanBeReplacedWithOrdinal_IfAppliedToParenthesizedQuery()
        {
            var query = @"
                (
                    SELECT  e.FirstName + ' ' + e.LastName
                    FROM    Employees e
                )
                ORDER   BY e.FirstName /* marker */ + ' ' + e.LastName
            ";

            var fixedQuery = @"
                (
                    SELECT  e.FirstName + ' ' + e.LastName
                    FROM    Employees e
                )
                ORDER   BY 1
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.AreEqual("Replace expression by SELECT column reference", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [TestMethod]
        public void OrderByExpressions_FindsUsageOfExpressionThatCanBeReplacedWithAlias_IfAppliedToParenthesizedQuery()
        {
            var query = @"
                (
                    SELECT  e.FirstName + ' ' + e.LastName AS FullName
                    FROM    Employees e
                )
                ORDER   BY e.FirstName /* marker */ + ' ' + e.LastName
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual(CodeIssueKind.Warning, issues[0].Kind);
            Assert.AreEqual("e.FirstName /* marker */ + ' ' + e.LastName", query.Substring(issues[0].Span));
        }

        [TestMethod]
        public void OrderByExpressions_FixesUsageOfExpressionThatCanBeReplacedWithAlias_IfAppliedToParenthesizedQuery()
        {
            var query = @"
                (
                    SELECT  e.FirstName + ' ' + e.LastName AS FullName
                    FROM    Employees e
                )
                ORDER   BY e.FirstName /* marker */ + ' ' + e.LastName
            ";

            var fixedQuery = @"
                (
                    SELECT  e.FirstName + ' ' + e.LastName AS FullName
                    FROM    Employees e
                )
                ORDER   BY FullName
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.AreEqual("Replace expression by SELECT column reference", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [TestMethod]
        public void OrderByExpressions_FindsUsageOfExpressionThatCanBeReplacedWithOrdinal_IfAppliedToUnionQuery()
        {
            var query = @"
                (
                    SELECT  e1.FirstName + ' ' + e1.LastName
                    FROM    Employees e1

                    UNION

                    SELECT  e2.FirstName + ' ' + e2.LastName
                    FROM    Employees e2               
                )
                ORDER   BY e1.FirstName /* marker */ + ' ' + e1.LastName
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual(CodeIssueKind.Warning, issues[0].Kind);
            Assert.AreEqual("e1.FirstName /* marker */ + ' ' + e1.LastName", query.Substring(issues[0].Span));
        }

        [TestMethod]
        public void OrderByExpressions_FixesUsageOfExpressionThatCanBeReplacedWithOrdinal_IfAppliedToUnionQuery()
        {
            var query = @"
                (
                    SELECT  e1.FirstName + ' ' + e1.LastName
                    FROM    Employees e1

                    UNION

                    SELECT  e2.FirstName + ' ' + e2.LastName
                    FROM    Employees e2               
                )
                ORDER   BY e1.FirstName /* marker */ + ' ' + e1.LastName
            ";

            var fixedQuery = @"
                (
                    SELECT  e1.FirstName + ' ' + e1.LastName
                    FROM    Employees e1

                    UNION

                    SELECT  e2.FirstName + ' ' + e2.LastName
                    FROM    Employees e2               
                )
                ORDER   BY 1
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.AreEqual("Replace expression by SELECT column reference", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [TestMethod]
        public void OrderByExpressions_FindsUsageOfExpressionThatCanBeReplacedWithAlias_IfAppliedToUnionQuery()
        {
            var query = @"
                (
                    SELECT  e1.FirstName + ' ' + e1.LastName AS FullName
                    FROM    Employees e1

                    UNION

                    SELECT  e2.FirstName + ' ' + e2.LastName
                    FROM    Employees e2               
                )
                ORDER   BY e1.FirstName /* marker */ + ' ' + e1.LastName
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);
            Assert.AreEqual(CodeIssueKind.Warning, issues[0].Kind);
            Assert.AreEqual("e1.FirstName /* marker */ + ' ' + e1.LastName", query.Substring(issues[0].Span));
        }

        [TestMethod]
        public void OrderByExpressions_FixesUsageOfExpressionThatCanBeReplacedWithAlias_IfAppliedToUnionQuery()
        {
            var query = @"
                (
                    SELECT  e1.FirstName + ' ' + e1.LastName AS FullName
                    FROM    Employees e1

                    UNION

                    SELECT  e2.FirstName + ' ' + e2.LastName
                    FROM    Employees e2               
                )
                ORDER   BY e1.FirstName /* marker */ + ' ' + e1.LastName
            ";

            var fixedQuery = @"
                (
                    SELECT  e1.FirstName + ' ' + e1.LastName AS FullName
                    FROM    Employees e1

                    UNION

                    SELECT  e2.FirstName + ' ' + e2.LastName
                    FROM    Employees e2               
                )
                ORDER   BY FullName
            ";

            var issues = GetIssues(query);
            Assert.AreEqual(1, issues.Length);

            var action = issues.First().Actions.First();
            Assert.AreEqual("Replace expression by SELECT column reference", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }
    }
}