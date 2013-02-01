using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Syntax;

namespace NQuery.UnitTests
{
    [TestClass]
    public class ParserTests
    {
        [TestMethod]
        public void Parser_SkipsBadTokens()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 'First' + !'Last' AS Name");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            var query = (SelectQuerySyntax) syntaxTree.Root.Root;
            var column = (ExpressionSelectColumnSyntax) query.SelectClause.Columns.Single();
            var binaryExpression = (BinaryExpressionSyntax) column.Expression;
            var left = (LiteralExpressionSyntax) binaryExpression.Left;
            var right = (LiteralExpressionSyntax) binaryExpression.Right;

            var skippedTokenTrivia = (SkippedTokensTriviaSyntax) right.Token.LeadingTrivia.Single().Structure;
            var skippedToken = skippedTokenTrivia.Tokens.Single();

            Assert.AreEqual("First", left.Value);
            Assert.AreEqual("Last", right.Value);

            Assert.AreEqual(SyntaxKind.BadToken, skippedToken.Kind);
            Assert.AreEqual("!", skippedToken.ValueText);

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.IllegalInputCharacter, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Parser_DetectsErrorAtEnd()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 'foo' WITH");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }
    }
}