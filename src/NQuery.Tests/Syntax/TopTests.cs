using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.UnitTests.Syntax
{
    public class TopTests
    {
        [Fact]
        public void Top_WithMissingWithKeyword_IsParsedCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT TOP 1 TIES NULL");
            var query = (SelectQuerySyntax) syntaxTree.Root.Root;
            var diagnostics = syntaxTree.GetDiagnostics().ToImmutableArray();

            Assert.False(query.SelectClause.TopClause.TiesKeyword.IsMissing);
            Assert.True(query.SelectClause.TopClause.WithKeyword.IsMissing);
            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Top_WithMissingTiesKeyword_IsParsedCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT TOP 1 WITH NULL");
            var query = (SelectQuerySyntax)syntaxTree.Root.Root;
            var diagnostics = syntaxTree.GetDiagnostics().ToImmutableArray();

            Assert.True(query.SelectClause.TopClause.TiesKeyword.IsMissing);
            Assert.False(query.SelectClause.TopClause.WithKeyword.IsMissing);
            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Top_WithInvalidInt_IsParsedAndBoundCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT TOP 1.5 NULL");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var smanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(smanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.InvalidInteger, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void Top_WithInvalidLiteral_IsParsedAndBoundCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT TOP 'text'");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var smanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(smanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.TokenExpected, diagnostics[0].DiagnosticId);
        }
    }
}