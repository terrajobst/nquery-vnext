using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    [TestClass]
    public class SortOrderTests : RefactoringTests
    {
        protected override ICodeRefactoringProvider CreateProvider()
        {
            return new SortOrderCodeRefactoringProvider();
        }

        [TestMethod]
        public void SortOrder_CanConvertImplicitToExplicit()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName| /*after*/
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName ASC /*after*/
            ";

            var actions = GetActions(query);
            Assert.AreEqual(2, actions.Length);

            var action = actions[0];
            Assert.AreEqual("To explicit sort order", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [TestMethod]
        public void SortOrder_CanConvertImplicitToDescending()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName| /*after*/
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName DESC /*after*/
            ";

            var actions = GetActions(query);
            Assert.AreEqual(2, actions.Length);
        
            var action = actions[1];
            Assert.AreEqual("To descending", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [TestMethod]
        public void SortOrder_CanConvertExplicitToImplicit()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName ASC| /*after*/
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName /*after*/
            ";

            var actions = GetActions(query);
            Assert.AreEqual(2, actions.Length);

            var action = actions[0];
            Assert.AreEqual("To implicit sort order", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }

        [TestMethod]
        public void SortOrder_CanConvertExplicitToAscending()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName ASC| /*after*/
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   BY e.FirstName DESC /*after*/
            ";

            var actions = GetActions(query);
            Assert.AreEqual(2, actions.Length);
        
            var action = actions[1];
            Assert.AreEqual("To descending", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }
    }
}