using System.Collections.Immutable;

using Xunit;

namespace NQuery.Tests.Binding
{
    public class ComparerTests
    {
        [Fact]
        public void SelectDistinct_RequiresAllColumnsToBeComparable()
        {
            var source = @"SELECT DISTINCT * FROM Table";
            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.InvalidDataTypeInSelectDistinct, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void SelectDistinct_RequiresAllColumnsToBeComparable_UnlessAllIsSpecified()
        {
            var source = @"SELECT ALL * FROM Table";
            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.Empty(diagnostics);
        }

        [Fact]
        public void GroupByAndAggregation_RequiresAllColumnsToBeComparable()
        {
            var source = @"SELECT Data FROM Table GROUP BY Data";
            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.InvalidDataTypeInGroupBy, diagnostics[0].DiagnosticId);
        }

        [Fact]
        public void OrderBy_RequiresAllColumnsToBeComparable()
        {
            var source = @"SELECT Data FROM Table ORDER BY Data";
            var syntaxTree = SyntaxTree.ParseQuery(source);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameDataTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.InvalidDataTypeInOrderBy, diagnostics[0].DiagnosticId);
        }

        [Fact]
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

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.InvalidDataTypeInUnion, diagnostics[0].DiagnosticId);
        }

        [Fact]
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

            Assert.Empty(diagnostics);
        }

        [Fact]
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

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.InvalidDataTypeInExcept, diagnostics[0].DiagnosticId);
        }

        [Fact]
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

            Assert.Single(diagnostics);
            Assert.Equal(DiagnosticId.InvalidDataTypeInIntersect, diagnostics[0].DiagnosticId);
        }
    }
}