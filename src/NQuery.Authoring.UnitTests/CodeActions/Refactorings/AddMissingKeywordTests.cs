using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.CodeActions;

namespace NQuery.Authoring.UnitTests.CodeActions.Refactorings
{
    [TestClass]
    public class AddMissingKeywordTests
    {
        private static ICodeAction[] GetActions(string query)
        {
            int position;
            var compilation = CompilationFactory.CreateQuery(query, out position);
            var semanticModel = compilation.GetSemanticModel();

            var provider = new AddMissingKeywordCodeRefactoringProvider();
            return provider.GetRefactorings(semanticModel, position).ToArray();
        }

        [TestMethod]
        public void AddMissingKeyword_DetectedForOrderWithoutBy()
        {
            var query = @"
                SELECT  *
                FROM    Employees e
                ORDER   /* before */ |e.FirstName
            ";

            var fixedQuery = @"
                SELECT  *
                FROM    Employees e
                ORDER   /* before */ BY e.FirstName
            ";

            var actions = GetActions(query);
            Assert.AreEqual(1, actions.Length);

            var action = actions.Single();
            var syntaxTree = action.GetEdit();
            Assert.AreEqual(fixedQuery, syntaxTree.TextBuffer.Text);
        }
    }
}