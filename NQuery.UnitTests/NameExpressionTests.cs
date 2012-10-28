using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace NQuery.Language.UnitTests
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
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

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
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvocationRequiresParenthesis, diagnostics[0].DiagnosticId);
        }
    }
}