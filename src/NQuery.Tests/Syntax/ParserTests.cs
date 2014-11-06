using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.UnitTests.Syntax
{
    public class ParserTests
    {
        [Fact]
        public void Parser_SkipsBadTokens()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 'First' + !'Last' AS Name");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            var query = (SelectQuerySyntax) syntaxTree.Root.Root;
            var column = (ExpressionSelectColumnSyntax) query.SelectClause.Columns.Single();
            var binaryExpression = (BinaryExpressionSyntax) column.Expression;
            var left = (LiteralExpressionSyntax) binaryExpression.Left;
            var right = (LiteralExpressionSyntax) binaryExpression.Right;

            var skippedTokenTrivia = (SkippedTokensTriviaSyntax) right.Token.LeadingTrivia.Single().Structure;
            var skippedToken = skippedTokenTrivia.Tokens.Single();

            Assert.Equal("First", left.Value);
            Assert.Equal("Last", right.Value);

            Assert.Equal(SyntaxKind.BadToken, skippedToken.Kind);
            Assert.Equal("!", skippedToken.ValueText);

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.IllegalInputCharacter, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Parser_DetectsErrorAtEnd()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 'foo' WITH");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }
    }
}