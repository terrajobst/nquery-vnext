using System;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Syntax;

namespace NQuery.UnitTests
{
    [TestClass]
    public class CommonTableExpressions
    {
        [TestMethod]
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

            Assert.AreEqual(2, cteSymbol.Columns.Count);
            Assert.AreEqual("A", cteSymbol.Columns[0].Name);
            Assert.AreEqual("B", cteSymbol.Columns[1].Name);
        }

        [TestMethod]
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

            Assert.AreEqual(2, cteSymbol.Columns.Count);
            Assert.AreEqual("C1", cteSymbol.Columns[0].Name);
            Assert.AreEqual("C2", cteSymbol.Columns[1].Name);
        }

        [TestMethod]
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

            var cteNodes = syntaxTree.Root.DescendantNodes().OfType<CommonTableExpressionSyntax>().ToArray();
            var cteSymbols = cteNodes.Select(semanticModel.GetDeclaredSymbol).ToArray();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            var tableInstances = (from n in cteNodes
                                  from tr in n.DescendantNodes().OfType<NamedTableReferenceSyntax>()
                                  select semanticModel.GetDeclaredSymbol(tr)).ToArray();

            Assert.AreEqual(3, cteSymbols.Length);
            Assert.AreEqual("MyCte1", cteSymbols[0].Name);
            Assert.AreEqual("MyCte2", cteSymbols[1].Name);
            Assert.AreEqual("MyCte3", cteSymbols[2].Name);

            Assert.AreEqual(3, tableInstances.Length);
            Assert.AreEqual(cteSymbols[0], tableInstances[0].Table);
            Assert.AreEqual(cteSymbols[0], tableInstances[1].Table);
            Assert.AreEqual(cteSymbols[1], tableInstances[2].Table);

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
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
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            var tableInstance = semanticModel.GetDeclaredSymbol(cteNode.DescendantNodes().OfType<NamedTableReferenceSyntax>().Single(r => r.TableName.ValueText == "MyCte"));

            Assert.AreEqual(cteSymbol, tableInstance.Table);

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
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
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            var tableInstance = semanticModel.GetDeclaredSymbol(cteNode.DescendantNodes().OfType<NamedTableReferenceSyntax>().Single(r => r.TableName.ValueText == "MyCte"));

            Assert.AreEqual(cteSymbol, tableInstance.Table);

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
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

            var diagnostics = semanticModel.GetDiagnostics().ToArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.NoColumnAliasSpecified);

            Assert.AreEqual(1, cteSymbol.Columns.Count);
            Assert.AreEqual("C2", cteSymbol.Columns[0].Name);

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual("No column name was specified for column 1 of 'MyCte'.", error.Message);
        }

        [TestMethod]
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

            var diagnostics = semanticModel.GetDiagnostics().ToArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.CteHasDuplicateColumnName);

            Assert.AreEqual(2, cteSymbol.Columns.Count);
            Assert.AreEqual("A", cteSymbol.Columns[0].Name);
            Assert.AreEqual(typeof(int), cteSymbol.Columns[0].Type);
            Assert.AreEqual("A", cteSymbol.Columns[1].Name);
            Assert.AreEqual(typeof(string), cteSymbol.Columns[1].Type);

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual("The column 'A' was specified multiple times for 'MyCte'.", error.Message);
        }

        [TestMethod]
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

            var diagnostics = semanticModel.GetDiagnostics().ToArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.CteHasDuplicateColumnName);

            Assert.AreEqual(2, cteSymbol.Columns.Count);
            Assert.AreEqual("A", cteSymbol.Columns[0].Name);
            Assert.AreEqual(typeof(int), cteSymbol.Columns[0].Type);
            Assert.AreEqual("A", cteSymbol.Columns[1].Name);
            Assert.AreEqual(typeof(string), cteSymbol.Columns[1].Type);

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual("The column 'A' was specified multiple times for 'MyCte'.", error.Message);
        }

        [TestMethod]
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
                              select semanticModel.GetDeclaredSymbol(n)).ToArray();

            var diagnostics = semanticModel.GetDiagnostics().ToArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.CteHasDuplicateTableName);

            Assert.AreEqual(2, cteSymbols.Length);
            Assert.AreEqual("MyCte", cteSymbols[0].Name);
            Assert.AreEqual("One", cteSymbols[0].Columns[0].Name);
            Assert.AreEqual("MyCte", cteSymbols[1].Name);
            Assert.AreEqual("Two", cteSymbols[1].Columns[0].Name);

            Assert.AreEqual("Duplicate common table expression name 'MyCte' was specified.", error.Message);
        }

        [TestMethod]
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

            var diagnostics = semanticModel.GetDiagnostics().ToArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.CteHasFewerColumnsThanSpecified);

            Assert.AreEqual(1, cteSymbol.Columns.Count);
            Assert.AreEqual("A", cteSymbol.Columns[0].Name);

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual("'MyCte' has fewer columns than were specified in the column list.", error.Message);
        }

        [TestMethod]
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

            var diagnostics = semanticModel.GetDiagnostics().ToArray();
            var error = diagnostics.Single(d => d.DiagnosticId == DiagnosticId.CteHasMoreColumnsThanSpecified);

            Assert.AreEqual(1, cteSymbol.Columns.Count);
            Assert.AreEqual("A", cteSymbol.Columns[0].Name);

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual("'MyCte' has more columns than were specified in the column list.", error.Message);
        }

        [TestMethod]
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

            Assert.AreEqual("Recursive common table expression 'MyCte' does not contain a top-level UNION ALL operator.", error.Message);
        }

        [TestMethod]
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
            Assert.AreEqual("No anchor member was specified for recursive query 'MyCte'.", error.Message);
        }

        [TestMethod]
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

            Assert.AreEqual(cteSymbol, tableInstance.Table);

            Assert.AreEqual("Recursive common table expression 'MyCte' does not contain a top-level UNION ALL operator.", error.Message);
        }

    }
}