using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.Language.UnitTests
{
    [TestClass]
    public sealed class BinaryOperatorExpressionTests
    {
        [TestMethod]
        public void BinaryOperator_AddIsParsedCorrectly()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x + y + z");

            var root = (BinaryExpressionSyntax)syntaxTree.Root.Root;
            var xplusy = (BinaryExpressionSyntax)root.Left;
            var x = (NameExpressionSyntax)xplusy.Left;
            var y = (NameExpressionSyntax)xplusy.Right;
            var z = (NameExpressionSyntax)root.Right;

            Assert.AreEqual("x", x.Name.ValueText);
            Assert.AreEqual("y", y.Name.ValueText);
            Assert.AreEqual("z", z.Name.ValueText);
        }

        [TestMethod]
        public void BinaryOperator_MultiplicationIsParsedBeforeAddition()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x + y * z");

            var root = (BinaryExpressionSyntax)syntaxTree.Root.Root;
            var x = (NameExpressionSyntax)root.Left;
            var ybyz = (BinaryExpressionSyntax)root.Right;
            var y = (NameExpressionSyntax)ybyz.Left;
            var z = (NameExpressionSyntax)ybyz.Right;

            Assert.AreEqual("x", x.Name.ValueText);
            Assert.AreEqual("y", y.Name.ValueText);
            Assert.AreEqual("z", z.Name.ValueText);
        }

        [TestMethod]
        public void BinaryOperator_DoesNotCauseCascadingErrors_WhenBothOperandsAreUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x + y");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();

            Assert.AreEqual(2, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[1].DiagnosticId);
        }

        [TestMethod]
        public void BinaryOperator_DoesNotCauseCascadingErrors_WhenLeftOperandsIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("x + 1");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void BinaryOperator_DoesNotCauseCascadingErrors_WhenRightOperandsIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("1 + y");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}