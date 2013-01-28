using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests
{
    [TestClass]
    public class OrderByTests
    {
        [TestMethod]
        public void OrderBy_DisallowsPosition_WhenZeroOrExceedingCount()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.Id, t.Name FROM Table t ORDER BY 0, 3");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(2, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.OrderByColumnPositionIsOutOfRange, diagnostics[0].DiagnosticId);
            Assert.AreEqual(DiagnosticId.OrderByColumnPositionIsOutOfRange, diagnostics[1].DiagnosticId);
        }

        [TestMethod]
        public void OrderBy_BindsByPosition()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.Id, t.Name FROM Table t ORDER BY 1, 2");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();
            var selectColumns = syntaxTree.Root.DescendantNodes()
                                          .OfType<ExpressionSelectColumnSyntax>()
                                          .Select(semanticModel.GetDeclaredSymbol)
                                          .ToArray();
            var orderBySymbols = syntaxTree.Root.DescendantNodes()
                                           .OfType<OrderByColumnSyntax>()
                                           .Select(semanticModel.GetSymbol)
                                           .ToArray();

            Assert.AreEqual(0, diagnostics.Length);
            CollectionAssert.AreEqual(selectColumns, orderBySymbols);
        }

        [TestMethod]
        public void OrderBy_BindsByName()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.Id AS Name, t.Name AS Id FROM Table t ORDER BY Name");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();
            var selectColumn = syntaxTree.Root.DescendantNodes()
                                         .OfType<ExpressionSelectColumnSyntax>()
                                         .Select(semanticModel.GetDeclaredSymbol)
                                         .FirstOrDefault();
            var orderByColumn = syntaxTree.Root.DescendantNodes()
                                           .OfType<OrderByColumnSyntax>()
                                           .Select(semanticModel.GetSymbol)
                                           .FirstOrDefault();
            var orderByColumnSelector = syntaxTree.Root.DescendantNodes()
                                           .OfType<OrderByColumnSyntax>()
                                           .Select(c => semanticModel.GetSymbol(c.ColumnSelector))
                                           .FirstOrDefault();

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(selectColumn, orderByColumn);
            Assert.AreEqual(selectColumn, orderByColumnSelector);
        }

        [TestMethod]
        public void OrderBy_BindsByStructure()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.Id + t.Name FROM Table t ORDER BY t.Id + t.Name");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();
            var selectColumn = syntaxTree.Root.DescendantNodes()
                                         .OfType<ExpressionSelectColumnSyntax>()
                                         .Select(semanticModel.GetDeclaredSymbol)
                                         .FirstOrDefault();
            var orderByColumn = syntaxTree.Root.DescendantNodes()
                                           .OfType<OrderByColumnSyntax>()
                                           .Select(semanticModel.GetSymbol)
                                           .FirstOrDefault();

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(selectColumn, orderByColumn);
        }

        [TestMethod]
        public void OrderBy_BindsNewExpression_WhenInSelect()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.Id, t.Name FROM Table t ORDER BY t.Id + t.Name");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();
            var orderByColumn = syntaxTree.Root.DescendantNodes()
                                           .OfType<OrderByColumnSyntax>()
                                           .Select(semanticModel.GetSymbol)
                                           .FirstOrDefault();

            Assert.AreEqual(0, diagnostics.Length);
            Assert.AreEqual(null, orderByColumn);
        }

        [TestMethod]
        public void OrderBy_DisallowsUsageOfAliasesAsNames()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.Name AS Foo FROM Table t ORDER BY LEN(Foo)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.ColumnTableOrVariableNotDeclared, diagnostics[0].DiagnosticId);
        }
    }
}