using System.Collections.Immutable;

using NQuery.Syntax;

namespace NQuery.Tests.Binding
{
    public class CombinedQueryTest
    {
        private static void AssertBindsToCommonTypes<T>(string queryCombinator)
            where T: QuerySyntax
        {
            var query = $@"
                SELECT  e.EmployeeID * 2.0,
                        e.LastName,
                        e.FirstName,
                        e.ReportsTo
                FROM    Employees e

                {queryCombinator}

                SELECT  e.EmployeeID,
                        e.LastName,
                        e.FirstName,
                        e.ReportsTo * 2.0
                FROM    Employees e
            ";

            var compilation = CompilationFactory.CreateQuery(query);
            var combinedQuery = compilation.SyntaxTree.Root.DescendantNodes().OfType<T>().Single();
            var semanticModel = compilation.GetSemanticModel();

            var columns = semanticModel.GetOutputColumns(combinedQuery).ToImmutableArray();

            Assert.Equal(4, columns.Length);
            Assert.Equal(typeof (double), columns[0].Type);
            Assert.Equal(typeof (string), columns[1].Type);
            Assert.Equal(typeof (string), columns[2].Type);
            Assert.Equal(typeof (double), columns[3].Type);
        }

        [Fact]
        public void UnionAll_BindsToCommonTypes()
        {
            AssertBindsToCommonTypes<UnionQuerySyntax>("UNION ALL");
        }

        [Fact]
        public void Union_BindsToCommonTypes()
        {
            AssertBindsToCommonTypes<UnionQuerySyntax>("UNION");
        }

        [Fact]
        public void Intersect_BindsToCommonTypes()
        {
            AssertBindsToCommonTypes<IntersectQuerySyntax>("INTERSECT");
        }

        [Fact]
        public void Except_BindsToCommonTypes()
        {
            AssertBindsToCommonTypes<ExceptQuerySyntax>("EXCEPT");
        }
    }
}