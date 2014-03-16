using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;
using NQuery.Authoring.CodeActions.Refactorings;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    [TestClass]
    public class AddAsAliasTests
    {
        private static ICodeAction[] GetActions(string query)
        {
            int position;
            var compilation = CompilationFactory.CreateQuery(query, out position);
            var semanticModel = compilation.GetSemanticModel();

            var provider = new AddAsAliasCodeRefactoringProvider();
            return provider.GetRefactorings(semanticModel, position).ToArray();
        }

        [TestMethod]
        public void AddAsAlias_DoesNotTrigger_WhenKeywordIsAlreadyPresent()
        {
            var query = @"
                SELECT  e.EmployeeID
                FROM    Employees AS e|
            ";

            var actions = GetActions(query);

            Assert.AreEqual(0, actions.Length);
        }

        [TestMethod]
        public void AddAsAlias_InsertsAs()
        {
            var query = @"
                SELECT  e.EmployeeID
                FROM    Employees /* before */ e| /* after */
            ";

            var fixedQuery = @"
                SELECT  e.EmployeeID
                FROM    Employees /* before */ AS e /* after */
            ";

            var actions = GetActions(query);
            Assert.AreEqual(1, actions.Length);

            var action = actions.Single();
            Assert.AreEqual("Add 'AS' keyword", action.Description);

            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.GetText());
        }
    }
}