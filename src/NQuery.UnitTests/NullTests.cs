using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace NQuery.UnitTests
{
    [TestClass]
    public class NullTests
    {
        [TestMethod]
        public void Null_SelectNullLiteralReturnsObjectNull()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT NULL");
            var compilation = Compilation.Empty.WithDataContext(DataContext.Default)
                                               .WithSyntaxTree(syntaxTree);

            using (var queryReader = compilation.GetQueryReader(false))
            {
                Assert.AreEqual(1, queryReader.ColumnCount);
                Assert.AreEqual(typeof(object), queryReader.GetColumnType(0));

                Assert.IsTrue(queryReader.Read());
                Assert.AreEqual(null, queryReader[0]);
                Assert.IsFalse(queryReader.Read());
            }
        }

        [TestMethod]
        public void Null_SelectNullExpressionReturnsTypedNull()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT 1.0 + NULL");
            var compilation = Compilation.Empty.WithDataContext(DataContext.Default)
                                               .WithSyntaxTree(syntaxTree);

            using (var queryReader = compilation.GetQueryReader(false))
            {
                Assert.AreEqual(1, queryReader.ColumnCount);
                Assert.AreEqual(typeof(double), queryReader.GetColumnType(0));

                Assert.IsTrue(queryReader.Read());
                Assert.AreEqual(null, queryReader[0]);
                Assert.IsFalse(queryReader.Read());
            }
        }

        [TestMethod]
        public void Null_SelectIsNullReturnsBooleanTrue()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT NULL IS NULL");
            var compilation = Compilation.Empty.WithDataContext(DataContext.Default)
                                               .WithSyntaxTree(syntaxTree);

            using (var queryReader = compilation.GetQueryReader(false))
            {
                Assert.AreEqual(1, queryReader.ColumnCount);
                Assert.AreEqual(typeof(bool), queryReader.GetColumnType(0));

                Assert.IsTrue(queryReader.Read());
                Assert.AreEqual(true, queryReader[0]);
                Assert.IsFalse(queryReader.Read());
            }
        }

        [TestMethod]
        public void Null_ImplicitConversionsFail_WhenAmbiguous()
        {
            var syntaxTree = SyntaxTree.ParseQuery("SELECT ROUND(NULL)");
            var compilation = Compilation.Empty.WithDataContext(DataContext.Default)
                                               .WithSyntaxTree(syntaxTree);

            var semanticModel = compilation.GetSemanticModel();
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToArray();

            Assert.AreEqual(1, diagnostics.Length);
            Assert.AreEqual(DiagnosticId.AmbiguousInvocation, diagnostics[0].DiagnosticId);
        }

        // TODO: Add this test case
        //SELECT *
        //FROM   Employees
        //WHERE  NULL --> Expect no errors

        // TODO: Add this test case
        //SELECT *
        //FROM   Employees e
        //            INNER JOIN EmployeeTerritories et ON NULL --> Expect no errors

        // TODO: Add this test case
        //SELECT 1, 2, 3
        //FROM   Employees
        //ORDER  BY NULL --> A constant expression was encountered in the ORDER BY list.

        // TODO: Add this test case
        //SELECT 1, 2, 3
        //FROM   Employees
        //GROUP  BY NULL  --> Each GROUP BY expression must contain at least one column that is not an outer reference.

        // TODO: Add this test case
        //SELECT SUM(EmployeeId)
        //FROM   Employees e
        //HAVING NULL --> Expect no errors
    }
}