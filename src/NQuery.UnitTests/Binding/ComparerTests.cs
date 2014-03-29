using System;
using System.Collections.Immutable;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NQuery.UnitTests.Binding
{
    [TestClass]
    public class ComparerTests
    {
        [TestMethod]
        public void SelectDistinct_RequiresAllColumnsToBeComparable()
        {
            var source = @"SELECT DISTINCT * FROM Table";
            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidDataTypeInSelectDistinct, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void SelectDistinct_RequiresAllColumnsToBeComparable_UnlessAllIsSpecified()
        {
            var source = @"SELECT ALL * FROM Table";
            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void GroupByAndAggregation_RequiresAllColumnsToBeComparable()
        {
            var source = @"SELECT Data FROM Table GROUP BY Data";
            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidDataTypeInGroupBy, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void OrderBy_RequiresAllColumnsToBeComparable()
        {
            var source = @"SELECT Data FROM Table ORDER BY Data";
            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidDataTypeInOrderBy, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Union_RequiresAllColumnsToBeComparable()
        {
            var source = @"
                SELECT * FROM Table
                UNION
                SELECT * FROM Table
            ";

            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidDataTypeInUnion, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Union_RequiresAllColumnsToBeComparable_UnlessAllIsSpecified()
        {
            var source = @"
                SELECT * FROM Table
                UNION ALL
                SELECT * FROM Table
            ";

            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void Except_RequiresAllColumnsToBeComparable()
        {
            var source = @"
                SELECT * FROM Table
                EXCEPT
                SELECT * FROM Table
            ";

            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidDataTypeInExcept, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Intersect_RequiresAllColumnsToBeComparable()
        {
            var source = @"
                SELECT * FROM Table
                INTERSECT
                SELECT * FROM Table
            ";

            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.InvalidDataTypeInIntersect, diagnostics[0].DiagnosticId);
        }
    }
}