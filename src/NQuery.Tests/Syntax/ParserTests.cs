using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.Tests.Syntax
{
    public partial class ParserTests
    {
        [Fact]
        public void Parser_Error_SkipsBadTokens()
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
        public void Parser_Error_DetectsErrorAtEnd()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 'foo' WITH");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Parser_Error_MissingIdentifier_IsInserted_IfKeywordOnNextLine()
        {
            const string text = @"
                SELECT   o.|
                FROM     Orders
            ";

            int position;
            var compilation = CompilationFactory.CreateQuery(text, out position);

            var dotToken = compilation.SyntaxTree.Root.FindToken(position, true);
            Assert.Equal(SyntaxKind.DotToken, dotToken.Kind);

            var identifierToken = dotToken.GetNextToken(true, true);
            Assert.Equal(SyntaxKind.IdentifierToken, identifierToken.Kind);
            Assert.True(identifierToken.IsMissing);
        }

        [Fact]
        public void Parser_Error_MissingIdentifier_IsInserted_AndSkipsKeyword_IfKeywordOnSameLine()
        {
            const string text = @"
                SELECT   o.Or|
                FROM     Orders
            ";

            int position;
            var compilation = CompilationFactory.CreateQuery(text, out position);

            var orKeyword = compilation.SyntaxTree.Root.FindToken(position, true);
            Assert.Equal(SyntaxKind.OrKeyword, orKeyword.Kind);

            var dotToken = orKeyword.GetPreviousToken(true, true);
            Assert.Equal(SyntaxKind.DotToken, dotToken.Kind);
            Assert.False(dotToken.IsMissing);

            var skippedTrivia = orKeyword.Parent.AncestorsAndSelf().OfType<SkippedTokensTriviaSyntax>().Single();
            var identifierToken = skippedTrivia.ParentTrivia.Parent;
            Assert.Equal(SyntaxKind.IdentifierToken, identifierToken.Kind);
            Assert.True(identifierToken.IsMissing);
        }
    }
}