using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests.Syntax
{
    [TestClass]
    public class ContextualLeftAndRightKeywordTests
    {
        [TestMethod]
        public void ContextualLeftAndRightKeywords_TreatedAsIdentifier_IfUsedAsName()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"SELECT LEFT, RIGHT");
            var tokens = syntaxTree.Root.DescendantTokens().ToArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.AreEqual(5, tokens.Length);

            // Now let's check whether we can use LEFT and RIGHT as identifiers.

            Assert.AreEqual("LEFT", tokens[1].Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, tokens[1].Kind);

            Assert.AreEqual("RIGHT", tokens[3].Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, tokens[3].Kind);
        }

        [TestMethod]
        public void ContextualLeftAndRightKeywords_TreatedAsIdentifier_IfUsedAsFunctionName()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"SELECT  LEFT(), RIGHT('test',2)");
            var tokens = syntaxTree.Root.DescendantTokens().ToArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.AreEqual(12, tokens.Length);

            // Now let's check whether we can use LEFT and RIGHT as identifiers.

            Assert.AreEqual("LEFT", tokens[1].Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, tokens[1].Kind);

            Assert.AreEqual("RIGHT", tokens[5].Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, tokens[5].Kind);
        }

        [TestMethod]
        public void ContextualLeftAndRightKeywords_TreatedAsIdentifier_IfUsedAsPropertyName()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"SELECT Foo.LEFT, Bar.RIGHT");
            var tokens = syntaxTree.Root.DescendantTokens().ToArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.AreEqual(9, tokens.Length);

            // Now let's check whether we can use LEFT and RIGHT as identifiers.

            Assert.AreEqual("LEFT", tokens[3].Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, tokens[3].Kind);

            Assert.AreEqual("RIGHT", tokens[7].Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, tokens[7].Kind);
        }

        [TestMethod]
        public void ContextualLeftAndRightKeywords_TreatedAsIdentifier_IfUsedAsMethodName()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"SELECT Foo.LEFT(), Bar.RIGHT('test', 2)");
            var tokens = syntaxTree.Root.DescendantTokens().ToArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.AreEqual(16, tokens.Length);

            // Now let's check whether we can use LEFT and RIGHT as identifiers.

            Assert.AreEqual("LEFT", tokens[3].Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, tokens[3].Kind);

            Assert.AreEqual("RIGHT", tokens[9].Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, tokens[7].Kind);
        }

        [TestMethod]
        public void ContextualLeftAndRightKeywords_TreatedAsIdentifier_IfUsedAsVariableName()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"SELECT @LEFT, @RIGHT");
            var tokens = syntaxTree.Root.DescendantTokens().ToArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.AreEqual(7, tokens.Length);

            // Now let's check whether we can use LEFT and RIGHT as identifiers.

            Assert.AreEqual("LEFT", tokens[2].Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, tokens[2].Kind);

            Assert.AreEqual("RIGHT", tokens[5].Text);
            Assert.AreEqual(SyntaxKind.IdentifierToken, tokens[5].Kind);
        }

        [TestMethod]
        public void ContextualLeftAndRightKeywords_TreatedAsKeyword_IfSucceededByJoinKeyword()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                SELECT  *
                FROM    T1 t1
                            LEFT JOIN T2 t2 ON t2.T1Id = t1.Id
                            RIGHT JOIN T3 t3 ON t3.Id = t2.T3Id
            ");
            var tokens = syntaxTree.Root.DescendantTokens().ToArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.AreEqual(30, tokens.Length);

            // Now let's make sure we still can use them as keywords in joins

            Assert.AreEqual("LEFT", tokens[5].Text);
            Assert.AreEqual(SyntaxKind.LeftKeyword, tokens[5].Kind);

            Assert.AreEqual("RIGHT", tokens[17].Text);
            Assert.AreEqual(SyntaxKind.RightKeyword, tokens[17].Kind);
        }

        [TestMethod]
        public void ContextualLeftAndRightKeywords_TreatedAsKeyword_IfSucceededByOuterKeyword()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                SELECT  *
                FROM    Employees e
                            LEFT OUTER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
                            RIGHT OUTER JOIN Territories t ON t.TerritoryId = et.TerritoryID
            ");
            var tokens = syntaxTree.Root.DescendantTokens().ToArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.AreEqual(32, tokens.Length);

            // Now let's make sure we still can use them as keywords in joins

            Assert.AreEqual("LEFT", tokens[5].Text);
            Assert.AreEqual(SyntaxKind.LeftKeyword, tokens[5].Kind);

            Assert.AreEqual("RIGHT", tokens[18].Text);
            Assert.AreEqual(SyntaxKind.RightKeyword, tokens[18].Kind);
        }

    }
}