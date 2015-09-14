using System;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.Tests.Syntax
{
    public class SeparatedSyntaxListTests
    {
        [Fact]
        public void SeparatedSyntaxList_Empty()
        {
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(new SyntaxNodeOrToken[0]);

            Assert.Equal(0, list.Count);
            Assert.False(list.Any());
        }

        [Fact]
        public void SeparatedSyntaxList_OneItem()
        {
            var expression = SyntaxFacts.ParseExpression("test");
            var nodeOrTokens = new SyntaxNodeOrToken[] { expression };
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(nodeOrTokens);

            Assert.Equal(1, list.Count);
            Assert.Equal(expression, Assert.Single(list));
            Assert.Empty(list.GetSeparators());
            Assert.Equal(null, list.GetSeparator(0));
        }

        [Fact]
        public void SeparatedSyntaxList_OneItem_WithTrailingComma()
        {
            var expression = SyntaxFacts.ParseExpression("test");
            var comma = SyntaxFacts.ParseToken(",");
            var nodeOrTokens = new SyntaxNodeOrToken[] { expression, comma };
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(nodeOrTokens);

            Assert.Equal(1, list.Count);
            Assert.Equal(expression, Assert.Single(list));
            Assert.Equal(new[] { comma }, list.GetSeparators());
            Assert.Equal(comma, list.GetSeparator(0));
        }

        [Fact]
        public void SeparatedSyntaxList_TwoItems()
        {
            var expression1 = SyntaxFacts.ParseExpression("test1");
            var comma = SyntaxFacts.ParseToken(",");
            var expression2 = SyntaxFacts.ParseExpression("test2");
            var nodeOrTokens = new SyntaxNodeOrToken[] { expression1, comma, expression2 };
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(nodeOrTokens);

            Assert.Equal(2, list.Count);
            Assert.Equal(new[] {expression1, expression2}, list);
            Assert.Equal(new[] { comma }, list.GetSeparators());
            Assert.Equal(comma, list.GetSeparator(0));
        }

        [Fact]
        public void SeparatedSyntaxList_TwoItems_WithTrailingComma()
        {
            var expression1 = SyntaxFacts.ParseExpression("test1");
            var comma1 = SyntaxFacts.ParseToken(",");
            var expression2 = SyntaxFacts.ParseExpression("test2");
            var comma2 = SyntaxFacts.ParseToken(",");
            var nodeOrTokens = new SyntaxNodeOrToken[] { expression1, comma1, expression2, comma2 };
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(nodeOrTokens);

            Assert.Equal(2, list.Count);
            Assert.Equal(new[] {expression1, expression2}, list);
            Assert.Equal(new[] { comma1, comma2 }, list.GetSeparators());
            Assert.Equal(comma1, list.GetSeparator(0));
            Assert.Equal(comma2, list.GetSeparator(1));
        }
    }
}