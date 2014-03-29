using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Syntax;

namespace NQuery.UnitTests.Syntax
{
    [TestClass]
    public sealed class BinaryOperatorExpressionTests
    {
        [TestMethod]
        public void BinaryOperator_AddIsParsedCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x + y + z");

            var root = (BinaryExpressionSyntax)syntaxTree.Root.Root;
            var xplusy = (BinaryExpressionSyntax)root.Left;
            var x = (NameExpressionSyntax)xplusy.Left;
            var y = (NameExpressionSyntax)xplusy.Right;
            var z = (NameExpressionSyntax)root.Right;

            Assert.AreEqual("x", x.Name.ValueText);
            Assert.AreEqual("y", y.Name.ValueText);
            Assert.AreEqual("z", z.Name.ValueText);
        }

        [TestMethod]
        public void BinaryOperator_MultiplicationIsParsedBeforeAddition()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x + y * z");

            var root = (BinaryExpressionSyntax)syntaxTree.Root.Root;
            var x = (NameExpressionSyntax)root.Left;
            var ybyz = (BinaryExpressionSyntax)root.Right;
            var y = (NameExpressionSyntax)ybyz.Left;
            var z = (NameExpressionSyntax)ybyz.Right;

            Assert.AreEqual("x", x.Name.ValueText);
            Assert.AreEqual("y", y.Name.ValueText);
            Assert.AreEqual("z", z.Name.ValueText);
        }
    }
}