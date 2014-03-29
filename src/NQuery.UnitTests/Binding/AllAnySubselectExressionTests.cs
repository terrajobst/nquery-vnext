using System;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Syntax;

namespace NQuery.UnitTests.Binding
{
    [TestClass]
    public sealed class AllAnySubselectExressionTests
    {
        [TestMethod]
        public void AllAnySubselect_DoesNotCauseCascadingErrors_WhenAnLeftIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("foo >= ALL (SELECT 'value')");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void AllAnySubselect_DoesNotCauseCascadingErrors_WhenAnRightIsUnresolved()
        {
            var syntaxTree = SyntaxTree.ParseExpression("'value' >= ALL (SELECT foo)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticMoel = compilation.GetSemanticModel();

            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticMoel.GetDiagnostics()).ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void AllAnySubselect_DetectsInvalidOperator()
        {
            var syntaxTree = SyntaxTree.ParseExpression("'value' + ALL (SELECT foo)");
            var diagnostics = syntaxTree.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidOperatorForAllAny, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void AllAnySubselect_ConvertsLeftSide()
        {
            var compilation = CompilationFactory.CreateExpression("1 >= ALL (SELECT EmployeeId * 2.0 FROM Employees)");
            var expresion = (ExpressionSyntax) compilation.SyntaxTree.Root.Root;
            var semanticModel = compilation.GetSemanticModel();
            var resultType = semanticModel.GetExpressionType(expresion);
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(typeof(bool), resultType);
            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void AllAnySubselect_ConvertsRightSide()
        {
            var compilation = CompilationFactory.CreateExpression("1.0 >= ALL (SELECT EmployeeId FROM Employees)");
            var expresion = (ExpressionSyntax) compilation.SyntaxTree.Root.Root;
            var semanticModel = compilation.GetSemanticModel();
            var resultType = semanticModel.GetExpressionType(expresion);
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(typeof(bool), resultType);
            Assert.AreEqual(0, diagnostics.Length);
        }
    }
}