using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.UnitTests.Binding
{
    public class SelectStarTests
    {
        [Fact]
        public void SelectStar_Disallowed_WhenNoTablesSpecified()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT *");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal(DiagnosticId.MustSpecifyTableToSelectFrom, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void SelectStar_Disallowed_WhenNoTablesSpecified_UnlessInExists()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 'Test' WHERE EXISTS (SELECT *)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.Equal(0, diagnostics.Length);
        }

        [Fact]
        public void SelectStar_HasAssociatedTable()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.* FROM Table t");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var selectStar = compilation.SyntaxTree.Root
                                        .DescendantNodes()
                                        .OfType<WildcardSelectColumnSyntax>()
                                        .Single();
            var tableInstance = compilation.SyntaxTree.Root
                                           .DescendantNodes()
                                           .OfType<NamedTableReferenceSyntax>()
                                           .Single();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            var tableReferenceSymbol = semanticModel.GetDeclaredSymbol(tableInstance);
            var selectStarSymbol = semanticModel.GetTableInstance(selectStar);

            Assert.Equal(0, diagnostics.Length);
            Assert.Equal(tableReferenceSymbol, selectStarSymbol);
        }

        [Fact]
        public void SelectStar_HasAssociatedTable_UnlessNoTableIsSpecified()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT * FROM Table t");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var selectStar = compilation.SyntaxTree.Root
                                        .DescendantNodes()
                                        .OfType<WildcardSelectColumnSyntax>()
                                        .Single();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            var selectStarSymbol = semanticModel.GetTableInstance(selectStar);

            Assert.Equal(0, diagnostics.Length);
            Assert.Equal(null, selectStarSymbol);
        }

        [Fact]
        public void SelectStar_HasAssociatedTableColumns()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT * FROM Table");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var selectStar = compilation.SyntaxTree.Root
                                        .DescendantNodes()
                                        .OfType<WildcardSelectColumnSyntax>()
                                        .Single();
            var tableInstance = compilation.SyntaxTree.Root
                                           .DescendantNodes()
                                           .OfType<NamedTableReferenceSyntax>()
                                           .Single();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            var tableSymbols = semanticModel.GetDeclaredSymbol(tableInstance).ColumnInstances;
            var selectStartSymbols = semanticModel.GetColumnInstances(selectStar).ToImmutableArray();

            Assert.Equal(0, diagnostics.Length);
            Assert.Equal(tableSymbols.AsEnumerable(), selectStartSymbols.AsEnumerable());
        }

        [Fact]
        public void SelectStar_HasAssociatedQueryTableColumns()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT * FROM Table");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var selectStar = compilation.SyntaxTree.Root
                                        .DescendantNodes()
                                        .OfType<WildcardSelectColumnSyntax>()
                                        .Single();
            var tableInstance = compilation.SyntaxTree.Root
                                           .DescendantNodes()
                                           .OfType<NamedTableReferenceSyntax>()
                                           .Single();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            var tableColumnNames = semanticModel.GetDeclaredSymbol(tableInstance)
                                                .ColumnInstances
                                                .Select(c => c.Name)
                                                .ToImmutableArray();
            var selectStarColumnNames = semanticModel.GetDeclaredSymbols(selectStar)
                                                     .Select(qc => qc.Name)
                                                     .ToImmutableArray();

            Assert.Equal(0, diagnostics.Length);
            Assert.Equal(tableColumnNames.AsEnumerable(), selectStarColumnNames.AsEnumerable());
        }
    }
}