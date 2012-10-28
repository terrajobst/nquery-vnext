using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace NQuery.Language.UnitTests
{
    [TestClass]
    public class SeparatedSyntaxListTests
    {
        [TestMethod]
        public void SeparatedSyntaxList_CanBeConstructedFromAnEmptyList()
        {
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(new SyntaxNodeOrToken[0]);

            Assert.AreEqual(0, list.Count);
            Assert.IsFalse(list.Any());
        }

        [TestMethod]
        public void SeparatedSyntaxList_CanBeConstructedFromSingleItem()
        {
            var expression = Helpers.ParseExpression("test");
            var nodeOrTokens = new SyntaxNodeOrToken[] { expression };
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(nodeOrTokens);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(expression, list.Single());
            Assert.AreEqual(null, list.GetSeparator(0));
        }

        [TestMethod]
        public void SeparatedSyntaxList_CanBeConstructedFromSingleItem_WithComma()
        {
            var expression = Helpers.ParseExpression("test");
            var comma = Helpers.CreateToken(SyntaxKind.CommaToken);
            var nodeOrTokens = new SyntaxNodeOrToken[] { expression, comma };
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(nodeOrTokens);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(expression, list.Single());
            Assert.AreEqual(comma.Kind, list.GetSeparator(0).Kind);
        }
    }
}