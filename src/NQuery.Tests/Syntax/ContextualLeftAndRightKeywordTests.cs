using System;
using System.Collections.Immutable;

using Xunit;

namespace NQuery.UnitTests.Syntax
{
    public class ContextualLeftAndRightKeywordTests
    {
        [Fact]
        public void ContextualLeftAndRightKeywords_TreatedAsIdentifier_IfUsedAsName()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"SELECT LEFT, RIGHT");
            var tokens = syntaxTree.Root.DescendantTokens().ToImmutableArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.Equal(5, tokens.Length);

            // Now let's check whether we can use LEFT and RIGHT as identifiers.

            Assert.Equal("LEFT", tokens[1].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[1].Kind);

            Assert.Equal("RIGHT", tokens[3].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[3].Kind);
        }

        [Fact]
        public void ContextualLeftAndRightKeywords_TreatedAsIdentifier_IfUsedAsFunctionName()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"SELECT  LEFT(), RIGHT('test',2)");
            var tokens = syntaxTree.Root.DescendantTokens().ToImmutableArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.Equal(12, tokens.Length);

            // Now let's check whether we can use LEFT and RIGHT as identifiers.

            Assert.Equal("LEFT", tokens[1].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[1].Kind);

            Assert.Equal("RIGHT", tokens[5].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[5].Kind);
        }

        [Fact]
        public void ContextualLeftAndRightKeywords_TreatedAsIdentifier_IfUsedAsPropertyName()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"SELECT Foo.LEFT, Bar.RIGHT");
            var tokens = syntaxTree.Root.DescendantTokens().ToImmutableArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.Equal(9, tokens.Length);

            // Now let's check whether we can use LEFT and RIGHT as identifiers.

            Assert.Equal("LEFT", tokens[3].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[3].Kind);

            Assert.Equal("RIGHT", tokens[7].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[7].Kind);
        }

        [Fact]
        public void ContextualLeftAndRightKeywords_TreatedAsIdentifier_IfUsedAsMethodName()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"SELECT Foo.LEFT(), Bar.RIGHT('test', 2)");
            var tokens = syntaxTree.Root.DescendantTokens().ToImmutableArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.Equal(16, tokens.Length);

            // Now let's check whether we can use LEFT and RIGHT as identifiers.

            Assert.Equal("LEFT", tokens[3].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[3].Kind);

            Assert.Equal("RIGHT", tokens[9].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[7].Kind);
        }

        [Fact]
        public void ContextualLeftAndRightKeywords_TreatedAsIdentifier_IfUsedAsVariableName()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"SELECT @LEFT, @RIGHT");
            var tokens = syntaxTree.Root.DescendantTokens().ToImmutableArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.Equal(7, tokens.Length);

            // Now let's check whether we can use LEFT and RIGHT as identifiers.

            Assert.Equal("LEFT", tokens[2].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[2].Kind);

            Assert.Equal("RIGHT", tokens[5].Text);
            Assert.Equal(SyntaxKind.IdentifierToken, tokens[5].Kind);
        }

        [Fact]
        public void ContextualLeftAndRightKeywords_TreatedAsKeyword_IfSucceededByJoinKeyword()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                SELECT  *
                FROM    T1 t1
                            LEFT JOIN T2 t2 ON t2.T1Id = t1.Id
                            RIGHT JOIN T3 t3 ON t3.Id = t2.T3Id
            ");
            var tokens = syntaxTree.Root.DescendantTokens().ToImmutableArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.Equal(30, tokens.Length);

            // Now let's make sure we still can use them as keywords in joins

            Assert.Equal("LEFT", tokens[5].Text);
            Assert.Equal(SyntaxKind.LeftKeyword, tokens[5].Kind);

            Assert.Equal("RIGHT", tokens[17].Text);
            Assert.Equal(SyntaxKind.RightKeyword, tokens[17].Kind);
        }

        [Fact]
        public void ContextualLeftAndRightKeywords_TreatedAsKeyword_IfSucceededByOuterKeyword()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                SELECT  *
                FROM    Employees e
                            LEFT OUTER JOIN EmployeeTerritories et ON et.EmployeeID = e.EmployeeID
                            RIGHT OUTER JOIN Territories t ON t.TerritoryId = et.TerritoryID
            ");
            var tokens = syntaxTree.Root.DescendantTokens().ToImmutableArray();

            // First's let's make sure we got the right amount of tokens.
            // If those don't line up, something else is broken.

            Assert.Equal(32, tokens.Length);

            // Now let's make sure we still can use them as keywords in joins

            Assert.Equal("LEFT", tokens[5].Text);
            Assert.Equal(SyntaxKind.LeftKeyword, tokens[5].Kind);

            Assert.Equal("RIGHT", tokens[18].Text);
            Assert.Equal(SyntaxKind.RightKeyword, tokens[18].Kind);
        }

    }
}