using System;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using NQuery.Syntax;

namespace NQuery.UnitTests.Binding
{
    [TestClass]
    public class CombinedQueryTest
    {
        private static void AssertBindsToCommonTypes<T>(string queryCombinator)
            where T: QuerySyntax
        {
            var query = string.Format(@"
                SELECT  e.EmployeeID * 2.0,
                        e.LastName,
                        e.FirstName,
                        e.ReportsTo
                FROM    Employees e

                {0}

                SELECT  e.EmployeeID,
                        e.LastName,
                        e.FirstName,
                        e.ReportsTo * 2.0
                FROM    Employees e
            ", queryCombinator);

            var compilation = CompilationFactory.CreateQuery(query);
            var combinedQuery = compilation.SyntaxTree.Root.DescendantNodes().OfType<T>().Single();
            var semanticModel = compilation.GetSemanticModel();

            var columns = semanticModel.GetOutputColumns(combinedQuery).ToImmutableArray();

            Assert.AreEqual(4, columns.Length);
            Assert.AreEqual(typeof (double), columns[0].Type);
            Assert.AreEqual(typeof (string), columns[1].Type);
            Assert.AreEqual(typeof (string), columns[2].Type);
            Assert.AreEqual(typeof (double), columns[3].Type);
        }

        [TestMethod]
        public void UnionAll_BindsToCommonTypes()
        {
            AssertBindsToCommonTypes<UnionQuerySyntax>("UNION ALL");
        }

        [TestMethod]
        public void Union_BindsToCommonTypes()
        {
            AssertBindsToCommonTypes<UnionQuerySyntax>("UNION");
        }

        [TestMethod]
        public void Intersect_BindsToCommonTypes()
        {
            AssertBindsToCommonTypes<IntersectQuerySyntax>("INTERSECT");
        }

        [TestMethod]
        public void Except_BindsToCommonTypes()
        {
            AssertBindsToCommonTypes<ExceptQuerySyntax>("EXCEPT");
        }
    }
}