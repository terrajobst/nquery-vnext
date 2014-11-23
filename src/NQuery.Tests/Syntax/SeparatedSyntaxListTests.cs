using System;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.Tests.Syntax
{
    public class SeparatedSyntaxListTests
    {
        [Fact]
        public void SeparatedSyntaxList_CanBeConstructedFromAnEmptyList()
        {
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(new SyntaxNodeOrToken[0]);

            Assert.Equal(0, list.Count);
            Assert.False(list.Any());
        }

        [Fact]
        public void SeparatedSyntaxList_CanBeConstructedFromSingleItem()
        {
            var expression = Helpers.ParseExpression("test");
            var nodeOrTokens = new SyntaxNodeOrToken[] { expression };
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(nodeOrTokens);

            Assert.Equal(1, list.Count);
            Assert.Equal(expression, list.Single());
            Assert.Equal(null, list.GetSeparator(0));
        }

        [Fact]
        public void SeparatedSyntaxList_CanBeConstructedFromSingleItem_WithComma()
        {
            var expression = Helpers.ParseExpression("test");
            var comma = Helpers.CreateToken(SyntaxKind.CommaToken);
            var nodeOrTokens = new SyntaxNodeOrToken[] { expression, comma };
            var list = new SeparatedSyntaxList<NameExpressionSyntax>(nodeOrTokens);

            Assert.Equal(1, list.Count);
            Assert.Equal(expression, list.Single());
            Assert.Equal(comma.Kind, list.GetSeparator(0).Kind);
        }
    }
}