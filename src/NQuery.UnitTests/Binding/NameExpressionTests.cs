using System;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests.Binding
{
    [TestClass]
    public class NameExpressionTests
    {
        [TestMethod]
        public void Name_DetectsMissingParentheses_WhenReferringToFunction()
        {
            var syntaxTree = SyntaxTree.ParseExpression("SUBSTRING");
            var compilation = Compilation.Empty
                .WithDataContext(DataContext.Default)
                .WithSyntaxTree(syntaxTree);

            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvocationRequiresParenthesis, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Name_DetectsMissingParentheses_WhenReferringToAggregate()
        {
            var syntaxTree = SyntaxTree.ParseExpression("MAX");
            var compilation = Compilation.Empty
                .WithDataContext(DataContext.Default)
                .WithSyntaxTree(syntaxTree);

            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvocationRequiresParenthesis, diagnostics[0].DiagnosticId);
        }
    }
}