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

//        [TestMethod]
//        public void Binder_DisallowsColumnsFromDifferentQueriesInAggregate()
//        {
//            var syntaxTree = SyntaxTree.ParseQuery(@"
//SELECT  COUNT(*)
//FROM    Table t1
//HAVING  1 >= ALL (
//            SELECT  SUM(t1.Id + t2.Id)
//            FROM    Table t2
//        )");
//            var compilation = Compilation.Empty.WithSyntaxTree(syntaxTree).WithIdNameTable();
//            var semanticModel = compilation.GetSemanticModel();
//            var diagnostics = semanticModel.GetDiagnostics().ToArray();

//            Assert.AreEqual(1, diagnostics.Length);
//            Assert.AreEqual(DiagnosticId.AggregateContainsColumnsFromDifferentQueries, diagnostics[0].DiagnosticId);
//        }

        // Aggregated and/or grouped queries:
        // * SELECT must not contain column instances that aren't grouped or aggregated
        // * HAVING must not contain column instances that aren't grouped or aggregated
        // * ORDER BY must not contain column instances that aren't grouped or aggregated
    }
}