using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Syntax;

using Xunit;

namespace NQuery.Tests.Binding
{
    public class CommonTableExpressions
    {
        [Fact]
        public void CTE_PreferColumnListOverQueryColumns()
        {
            const string query = @"
                WITH MyCte(A, B) AS
                (
                    SELECT 'x' AS C1, 'y' AS C2
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteSyntax = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().Single();
            var cteSymbol = semanticModel.GetDeclaredSymbol(cteSyntax);

            Assert.Equal(2, cteSymbol.Columns.Length);
            Assert.Equal("A", cteSymbol.Columns[0].Name);
            Assert.Equal("B", cteSymbol.Columns[1].Name);
        }

        [Fact]
        public void CTE_UseQueryColumnNames_IfNoColumnListIsSpecified()
        {
            const string query = @"
                WITH MyCte AS
                (
                    SELECT 'x' AS C1, 'y' AS C2
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteSyntax = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().Single();
            var cteSymbol = semanticModel.GetDeclaredSymbol(cteSyntax);

            Assert.Equal(2, cteSymbol.Columns.Length);
            Assert.Equal("C1", cteSymbol.Columns[0].Name);
            Assert.Equal("C2", cteSymbol.Columns[1].Name);
        }

        [Fact]
        public void CTE_CanBindToPreviouslyDefinedExpressions()
        {
            const string query = @"
                WITH MyCte1 AS
                (
                    SELECT 'x' AS C
                ), MyCte2 AS
                (
                    SELECT  'x' AS C
                    FROM    MyCte1
                ), MyCte3 AS
                (
                    SELECT 'x' AS C
                    FROM    MyCte1
                                CROSS JOIN MyCte2
                )
                SELECT  *
                FROM    MyCte3";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteNodes = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().ToImmutableArray();
            var cteSymbols = cteNodes.Select(semanticModel.GetDeclaredSymbol).ToImmutableArray();
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            var tableInstances = (from n in cteNodes
                                  from tr in n.DescendantNodes().OfType<NamedTableReferenceSyntax>()
                                  select semanticModel.GetDeclaredSymbol(tr)).ToImmutableArray();

            Assert.Equal(3, cteSymbols.Length);
            Assert.Equal("MyCte1", cteSymbols[0].Name);
            Assert.Equal("MyCte2", cteSymbols[1].Name);
            Assert.Equal("MyCte3", cteSymbols[2].Name);

            Assert.Equal(3, tableInstances.Length);
            Assert.Equal(cteSymbols[0], tableInstances[0].Table);
            Assert.Equal(cteSymbols[0], tableInstances[1].Table);
            Assert.Equal(cteSymbols[1], tableInstances[2].Table);

            Assert.Equal(0, diagnostics.Length);
        }

        [Fact]
        public void CTE_CanBindToSelf_WithSingleAnchor()
        {
            const string query = @"
                WITH MyCte AS
                (
	                SELECT	1 AS Id,
			                -1 AS ReportsTo

	                UNION ALL

	                SELECT d.Id,
		                   d.ReportsTo
	                FROM   (SELECT 2 AS Id, 1 AS ReportsTo
			                UNION ALL
			                SELECT 3, 1) d
				                INNER JOIN MyCte c ON c.Id = d.ReportsTo
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteNode = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().Single();
            var cteSymbol = semanticModel.GetDeclaredSymbol(cteNode);
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            var tableInstance = semanticModel.GetDeclaredSymbol(cteNode.DescendantNodes().OfType<NamedTableReferenceSyntax>().Single(r => r.TableName.ValueText == "MyCte"));

            Assert.Equal(cteSymbol, tableInstance.Table);

            Assert.Equal(0, diagnostics.Length);
        }

        [Fact]
        public void CTE_CanBindToSelf_WithMultipleAnchors()
        {
            const string query = @"
                WITH MyCte AS
                (
	                SELECT	1 AS Id,
			                -1 AS ReportsTo

	                UNION ALL

	                SELECT	2 AS Id,
			                -1 AS ReportsTo

	                UNION ALL

	                SELECT d.Id,
		                   d.ReportsTo
	                FROM   (SELECT 3 AS Id, 1 AS ReportsTo
			                UNION ALL
			                SELECT 4, 1) d
				                INNER JOIN MyCte c ON c.Id = d.ReportsTo
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteNode = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().Single();
            var cteSymbol = semanticModel.GetDeclaredSymbol(cteNode);
            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();

            var tableInstance = semanticModel.GetDeclaredSymbol(cteNode.DescendantNodes().OfType<NamedTableReferenceSyntax>().Single(r => r.TableName.ValueText == "MyCte"));

            Assert.Equal(cteSymbol, tableInstance.Table);

            Assert.Equal(0, diagnostics.Length);
        }

        [Fact]
        public void CTE_DetectColumnsWithoutNames_IfNoColumnListIsSpecified()
        {
            const string query = @"
                WITH MyCte AS
                (
                    SELECT 'x' AS, 'y' AS C2
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteSyntax = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().Single();
            var cteSymbol = semanticModel.GetDeclaredSymbol(cteSyntax);

            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.NoColumnAliasSpecified);

            Assert.Equal(1, cteSymbol.Columns.Length);
            Assert.Equal("C2", cteSymbol.Columns[0].Name);

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal("No column name was specified for column 1 of 'MyCte'.", error.Message);
        }

        [Fact]
        public void CTE_DetectDuplicateColumnNames()
        {
            const string query = @"
                WITH MyCte (A, A) AS
                (
                    SELECT 1 AS, '2' AS C2
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteSyntax = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().Single();
            var cteSymbol = semanticModel.GetDeclaredSymbol(cteSyntax);

            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.CteHasDuplicateColumnName);

            Assert.Equal(2, cteSymbol.Columns.Length);
            Assert.Equal("A", cteSymbol.Columns[0].Name);
            Assert.Equal(typeof(int), cteSymbol.Columns[0].Type);
            Assert.Equal("A", cteSymbol.Columns[1].Name);
            Assert.Equal(typeof(string), cteSymbol.Columns[1].Type);

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal("The column 'A' was specified multiple times for 'MyCte'.", error.Message);
        }

        [Fact]
        public void CTE_DetectDuplicateColumnNames_IfNoColumnListIsSpecified()
        {
            const string query = @"
                WITH MyCte AS
                (
                    SELECT 1 AS A, '2' AS A
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteSyntax = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().Single();
            var cteSymbol = semanticModel.GetDeclaredSymbol(cteSyntax);

            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.CteHasDuplicateColumnName);

            Assert.Equal(2, cteSymbol.Columns.Length);
            Assert.Equal("A", cteSymbol.Columns[0].Name);
            Assert.Equal(typeof(int), cteSymbol.Columns[0].Type);
            Assert.Equal("A", cteSymbol.Columns[1].Name);
            Assert.Equal(typeof(string), cteSymbol.Columns[1].Type);

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal("The column 'A' was specified multiple times for 'MyCte'.", error.Message);
        }

        [Fact]
        public void CTE_DetectDuplicateTableNames()
        {
            const string query = @"
                WITH MyCte AS
                (
                    SELECT 1 AS One
                ), MyCte AS
                (
                    SELECT '2' AS Two
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteSymbols = (from n in syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>()
                              select semanticModel.GetDeclaredSymbol(n)).ToImmutableArray();

            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.CteHasDuplicateTableName);

            Assert.Equal(2, cteSymbols.Length);
            Assert.Equal("MyCte", cteSymbols[0].Name);
            Assert.Equal("One", cteSymbols[0].Columns[0].Name);
            Assert.Equal("MyCte", cteSymbols[1].Name);
            Assert.Equal("Two", cteSymbols[1].Columns[0].Name);

            Assert.Equal("Duplicate common table expression name 'MyCte' was specified.", error.Message);
        }

        [Fact]
        public void CTE_DetectIfFewerColumnsInQueryThanSpecifiedInColumnList()
        {
            const string query = @"
                WITH MyCte(A, B) AS
                (
                    SELECT 'x' AS C1
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteSyntax = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().Single();
            var cteSymbol = semanticModel.GetDeclaredSymbol(cteSyntax);

            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.CteHasFewerColumnsThanSpecified);

            Assert.Equal(1, cteSymbol.Columns.Length);
            Assert.Equal("A", cteSymbol.Columns[0].Name);

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal("'MyCte' has fewer columns than were specified in the column list.", error.Message);
        }

        [Fact]
        public void CTE_DetectIfMoreColumnsInQueryThanSpecifiedInColumnList()
        {
            const string query = @"
                WITH MyCte(A) AS
                (
                    SELECT 'x' AS C1, 2 AS C2
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteSyntax = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().Single();
            var cteSymbol = semanticModel.GetDeclaredSymbol(cteSyntax);

            var diagnostics = semanticModel.GetDiagnostics().ToImmutableArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.CteHasMoreColumnsThanSpecified);

            Assert.Equal(1, cteSymbol.Columns.Length);
            Assert.Equal("A", cteSymbol.Columns[0].Name);

            Assert.Equal(1, diagnostics.Length);
            Assert.Equal("'MyCte' has more columns than were specified in the column list.", error.Message);
        }

        [Fact]
        public void CTE_DetectMissingUnionAll()
        {
            const string query = @"
                WITH MyCte AS
                (
                    SELECT * FROM MyCte
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var error = semanticModel.GetDiagnostics().Single(d => d.DiagnosticId == DiagnosticId.CteDoesNotHaveUnionAll);

            Assert.Equal("Recursive common table expression 'MyCte' does not contain a top-level UNION ALL operator.", error.Message);
        }

        [Fact]
        public void CTE_DetectMissingAnchorInUnionAll()
        {
            const string query = @"
                WITH MyCte AS
                (
	                SELECT d.Id,
		                   d.ReportsTo
	                FROM   (SELECT 2 AS Id, 1 AS ReportsTo
			                UNION ALL
			                SELECT 3, 1) d
				                INNER JOIN MyCte c ON c.Id = d.ReportsTo

	                UNION

	                SELECT d.Id,
		                   d.ReportsTo
	                FROM   (SELECT 2 AS Id, 1 AS ReportsTo
			                UNION ALL
			                SELECT 3, 1) d
				                INNER JOIN MyCte c ON c.Id = d.ReportsTo
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var error = semanticModel.GetDiagnostics().Single(d => d.DiagnosticId == DiagnosticId.CteDoesNotHaveAnchorMember);
            Assert.Equal("No anchor member was specified for recursive query 'MyCte'.", error.Message);
        }

        [Fact]
        public void CTE_DetectMissingUnionAll_ButContinuesForUnion()
        {
            const string query = @"
                WITH MyCte AS
                (
	                SELECT	1 AS Id,
			                -1 AS ReportsTo

	                UNION

	                SELECT d.Id,
		                   d.ReportsTo
	                FROM   (SELECT 2 AS Id, 1 AS ReportsTo
			                UNION ALL
			                SELECT 3, 1) d
				                INNER JOIN MyCte c ON c.Id = d.ReportsTo
                )
                SELECT  *
                FROM    MyCte";

            var syntaxTree = SyntaxTree.ParseQuery(query);
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree);
            var semanticModel = compilation.GetSemanticModel();

            var cteNode = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().Single();
            var cteSymbol = semanticModel.GetDeclaredSymbol(cteNode);

            var tableInstance = semanticModel.GetDeclaredSymbol(cteNode.DescendantNodes().OfType<NamedTableReferenceSyntax>().Single(r => r.TableName.ValueText == "MyCte"));

            var error = semanticModel.GetDiagnostics().Single(d => d.DiagnosticId == DiagnosticId.CteDoesNotHaveUnionAll);

            Assert.Equal(cteSymbol, tableInstance.Table);

            Assert.Equal("Recursive common table expression 'MyCte' does not contain a top-level UNION ALL operator.", error.Message);
        }
    }
}