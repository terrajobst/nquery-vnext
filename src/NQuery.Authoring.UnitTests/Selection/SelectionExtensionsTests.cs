using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Authoring.Selection;
using NQuery.Text;

namespace NQuery.Authoring.UnitTests.Selection
{
    [TestClass]
    public class SelectionExtensionsTests
    {
        [TestMethod]
        public void SelectionExtensions_Grows()
        {
            var query = @"
                SELECT  e.First|Name
                FROM    Employees e
            ";

            int position;
            var compilation = CompilationFactory.CreateQuery(query, out position);
            var syntaxTree = compilation.SyntaxTree;
            var textBuffer = syntaxTree.TextBuffer;
            var start = new TextSpan(position, 0);

            var firstTime = syntaxTree.ExtendSelection(start);
            Assert.AreEqual("FirstName", textBuffer.GetText(firstTime));

            var secondTime = syntaxTree.ExtendSelection(firstTime);
            Assert.AreEqual("e.FirstName", textBuffer.GetText(secondTime));

            var thirdTime = syntaxTree.ExtendSelection(secondTime);
            Assert.AreEqual("SELECT  e.FirstName", textBuffer.GetText(thirdTime));

            var fourthTime = syntaxTree.ExtendSelection(thirdTime);
            Assert.AreEqual(textBuffer.GetText().Trim(), textBuffer.GetText(fourthTime));

            var fifthTime = syntaxTree.ExtendSelection(fourthTime);
            Assert.AreEqual(textBuffer.GetText().TrimStart(), textBuffer.GetText(fifthTime));

            var sixthTime = syntaxTree.ExtendSelection(fifthTime);
            Assert.AreEqual(fifthTime, sixthTime);
        }
    }
}