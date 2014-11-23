using System;

using NQuery.Syntax;

using Xunit;

namespace NQuery.Tests.Syntax
{
    public sealed class BinaryOperatorExpressionTests
    {
        [Fact]
        public void BinaryOperator_AddIsParsedCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x + y + z");

            var root = (BinaryExpressionSyntax)syntaxTree.Root.Root;
            var xplusy = (BinaryExpressionSyntax)root.Left;
            var x = (NameExpressionSyntax)xplusy.Left;
            var y = (NameExpressionSyntax)xplusy.Right;
            var z = (NameExpressionSyntax)root.Right;

            Assert.Equal("x", x.Name.ValueText);
            Assert.Equal("y", y.Name.ValueText);
            Assert.Equal("z", z.Name.ValueText);
        }

        [Fact]
        public void BinaryOperator_MultiplicationIsParsedBeforeAddition()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x + y * z");

            var root = (BinaryExpressionSyntax)syntaxTree.Root.Root;
            var x = (NameExpressionSyntax)root.Left;
            var ybyz = (BinaryExpressionSyntax)root.Right;
            var y = (NameExpressionSyntax)ybyz.Left;
            var z = (NameExpressionSyntax)ybyz.Right;

            Assert.Equal("x", x.Name.ValueText);
            Assert.Equal("y", y.Name.ValueText);
            Assert.Equal("z", z.Name.ValueText);
        }
    }
}