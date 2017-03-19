using NQuery.Authoring.Selection;
using NQuery.Text;
using Xunit;

namespace NQuery.Authoring.Tests.Selection
{
    public class SelectionExtensionsTests : ExtensionTests
    {
        [Fact]
        public void SelectionExtensions_ReturnsAllProviders()
        {
            AssertAllProvidersAreExposed(SelectionExtensions.GetStandardSelectionSpanProviders);
        }

        [Fact]
        public void SelectionExtensions_Grows()
        {
            var query = @"
                SELECT  e.First|Name
                FROM    Employees e
            ";

            int position;
            var compilation = CompilationFactory.CreateQuery(query, out position);
            var syntaxTree = compilation.SyntaxTree;
            var text = syntaxTree.Text;
            var start = new TextSpan(position, 0);

            var firstTime = syntaxTree.ExtendSelection(start);
            Assert.Equal("FirstName", text.GetText(firstTime));

            var secondTime = syntaxTree.ExtendSelection(firstTime);
            Assert.Equal("e.FirstName", text.GetText(secondTime));

            var thirdTime = syntaxTree.ExtendSelection(secondTime);
            Assert.Equal("SELECT  e.FirstName", text.GetText(thirdTime));

            var fourthTime = syntaxTree.ExtendSelection(thirdTime);
            Assert.Equal(text.GetText().Trim(), text.GetText(fourthTime));

            var fifthTime = syntaxTree.ExtendSelection(fourthTime);
            Assert.Equal(text.GetText().TrimStart(), text.GetText(fifthTime));

            var sixthTime = syntaxTree.ExtendSelection(fifthTime);
            Assert.Equal(fifthTime, sixthTime);
        }
    }
}