using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace NQuery.UnitTests
{
    [TestClass]
    public class BinderTests
    {
        [TestMethod]
        public void Binder_DisallowsAggregateInAggregate()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(COUNT(*)) FROM Table");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AggregateCannotContainAggregate, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsAggregateInWhere()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 1 FROM Table WHERE COUNT(*) > 0");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AggregateInWhere, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsAggregateInGroupBy()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 1 FROM Table GROUP BY COUNT(*)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AggregateInGroupBy, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsAggregateInOn()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 1 FROM Table t1 INNER JOIN Table t2 ON t1.Id = COUNT(*)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AggregateInOn, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsSubqueryInGroupBy()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 1 FROM Table GROUP BY (SELECT NULL)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.GroupByCannotContainSubquery, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsSubqueryInAggregate()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT((SELECT NULL)) FROM Table");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AggregateCannotContainSubquery, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsColumnsFromDifferentQueriesInAggregate()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                SELECT  COUNT(*)
                FROM    Table t1
                HAVING  1 >= ALL (
                            SELECT  SUM(t1.Id + t2.Id)
                            FROM    Table t2
                        )
            ");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AggregateContainsColumnsFromDifferentQueries, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsSelectStarWithoutTables()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT *");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.MustSpecifyTableToSelectFrom, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsSelectStarWithoutTables_UnlessInExists()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 'Test' WHERE EXISTS (SELECT *)");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInSelect_WhenGrouped()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.Id FROM Table t GROUP BY t.Name");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.SelectExpressionNotAggregatedOrGrouped, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInSelect_WhenGrouped_UnlessAggregated()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                    SELECT      COUNT(t.Id)
                    FROM        Table t
                    GROUP BY    t.Name
            ");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInSelect_WhenGrouped_UnlessSameAsGroup()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                    SELECT      t.Name + ' ' + t.Id.ToString()
                    FROM        Table t
                    GROUP BY    t.Name + ' ' + t.Id.ToString()
            ");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInSelect_WhenCountStarIsPresent()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.Name, COUNT(*) FROM Table t");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.SelectExpressionNotAggregatedAndNoGroupBy, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInSelect_WhenAggregateIsPresent()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT t.Name, COUNT(t.Id) FROM Table t");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.SelectExpressionNotAggregatedAndNoGroupBy, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInSelect_UnlessBelongsToDifferentQuery()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                SELECT  COUNT(*)
                FROM    Table t1
                HAVING  1 >= ALL (
                            SELECT  SUM(t1.Id)
                            FROM    Table t2
                        )
            ");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInHaving_WhenGrouped()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 1 FROM Table t GROUP BY t.Name HAVING t.Id <> 1");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.HavingExpressionNotAggregatedOrGrouped, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInHaving_WhenGrouped_UnlessAggregated()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                    SELECT      1
                    FROM        Table t
                    GROUP BY    t.Name
                    HAVING      SUM(t.Id) > 10
            ");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInHaving_WhenGrouped_UnlessSameAsGroup()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                    SELECT      1
                    FROM        Table t
                    GROUP BY    t.Name
                    HAVING      t.Name <> 'test'
            ");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInHaving_WhenCountStarIsPresent()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(*) FROM Table t HAVING t.Name <> 'test'");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.HavingExpressionNotAggregatedOrGrouped, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInHaving_WhenAggregateIsPresent()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(t.Id) FROM Table t HAVING t.Name <> 'test'");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.HavingExpressionNotAggregatedOrGrouped, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInOrderBy_WhenGrouped()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 1 FROM Table t GROUP BY t.Name ORDER BY t.Id");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.OrderByExpressionNotAggregatedOrGrouped, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInOrderBy_WhenGrouped_UnlessAggregated()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                    SELECT      1
                    FROM        Table t
                    GROUP BY    t.Name
                    ORDER BY    SUM(t.Id)
            ");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInOrderBy_WhenGrouped_UnlessSameAsGroup()
        {
            var syntaxTree = SyntaxTree.ParseQuery(@"
                    SELECT      1
                    FROM        Table t
                    GROUP BY    t.Id, t.Name + '*'
                    ORDER BY    t.Name + '*'
            ");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(0, diagnostics.Length);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInOrderBy_WhenCountStarIsPresent()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(*) FROM Table t ORDER BY t.Name");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.OrderByExpressionNotAggregatedAndNoGroupBy, diagnostics[0].DiagnosticId);
        }

        [TestMethod]
        public void Binder_DisallowsColumnInstanceInOrderBy_WhenAggregateIsPresent()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT COUNT(t.Id) FROM Table t ORDER BY t.Name");
            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = semanticModel.GetDiagnostics().ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.OrderByExpressionNotAggregatedAndNoGroupBy, diagnostics[0].DiagnosticId);
        }

        //// TODO: ERROR
        //SELECT  *
        //FROM    Employees e
        //GROUP   BY e.EmployeeID
    }
}