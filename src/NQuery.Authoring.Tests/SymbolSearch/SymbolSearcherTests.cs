using System.Collections.Immutable;

using NQuery.Authoring.SymbolSearch;
using NQuery.Text;

namespace NQuery.Authoring.Tests.SymbolSearch
{
    public class SymbolSearcherTests
    {
        private static void AssertIsMatch(string sql)
        {
            ImmutableArray<TextSpan> textSpans;
            var query = sql.ParseSpans(out textSpans);

            var compilation = CompilationFactory.CreateQuery(query);
            var semanticModel = compilation.GetSemanticModel();

            foreach (var span in textSpans)
            {
                var symbolSpan = semanticModel.FindSymbol(span.End).Value;

                Assert.NotNull(symbolSpan.Symbol);
                Assert.Equal(span, symbolSpan.Span);

                var usageSymbolSpans = semanticModel.FindUsages(symbolSpan.Symbol).ToImmutableArray();
                var usageSpans = usageSymbolSpans.Select(s => s.Span).ToImmutableArray();

                foreach (var usageSymbolSpan in usageSymbolSpans)
                    Assert.Equal(usageSymbolSpan.Symbol, symbolSpan.Symbol);

                foreach (var usageSpan in usageSpans)
                    Assert.Contains(usageSpan, textSpans);

                foreach (var textSpan in textSpans)
                    Assert.Contains(textSpan, usageSpans);
            }
        }

        [Fact]
        public void SymbolSearcher_FindsColumns()
        {
            var sql = @"
                SELECT  e.{FirstName},
                        e.LastName
                FROM    Employees e
                WHERE   e.{FirstName} = 'Andrew'
                AND     e.LastName = 'Fuller'
                ORDER   BY LastName, FirstName
            ";

            AssertIsMatch(sql);
        }

        [Fact]
        public void SymbolSearcher_FindsColumnInstances()
        {
            var sql = @"
                SELECT  e.FirstName + ' ' + e.LastName AS {FullName}
                FROM    Employees e
                WHERE   e.City = 'London'
                ORDER   BY {FullName}
            ";

            AssertIsMatch(sql);
        }

        [Fact]
        public void SymbolSearcher_FindsTableInstances()
        {
            var sql = @"
                SELECT  {e}.FirstName,
                        {e}.LastName
                FROM    Employees {e}
            ";

            AssertIsMatch(sql);
        }

        [Fact]
        public void SymbolSearcher_FindsTableInstances_IfNoAlias()
        {
            var sql = @"
                SELECT  {Employees}.FirstName,
                        {Employees}.LastName
                FROM    Employees
            ";

            AssertIsMatch(sql);
        }

        [Fact]
        public void SymbolSearcher_FindsTables()
        {
            var sql = @"
                SELECT  e1.FirstName,
                        e1.LastName
                FROM    {Employees} e1
                            CROSS JOIN {Employees} e2
            ";

            AssertIsMatch(sql);
        }

        [Fact]
        public void SymbolSearcher_FindsDerivedTables()
        {
            var sql = @"
                SELECT  {d}.FirstName,
                        {d}.LastName
                FROM    (SELECT * FROM Employees) {d}
            ";

            AssertIsMatch(sql);
        }

        [Fact]
        public void SymbolSearcher_FindsCommonTableExpressions()
        {
            var sql = @"
                WITH {LondonEmps} AS
                (
                    SELECT  *
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  *
                FROM    {LondonEmps}
            ";

            AssertIsMatch(sql);
        }

        [Fact]
        public void SymbolSearcher_FindsProperties()
        {
            var sql = @"
                SELECT  e.FirstName.{Length},
                        e.LastName.{Length}
                FROM    Employees e
            ";

            AssertIsMatch(sql);
        }

        [Fact]
        public void SymbolSearcher_FindsMethods()
        {
            var sql = @"
                SELECT  e.EmployeeID.{ToString}().ToString(),
                        e.BirthDate.Year.{ToString}()
                FROM    Employees e
            ";

            AssertIsMatch(sql);
        }

        [Fact]
        public void SymbolSearcher_FindsCountAggregate()
        {
            var sql = @"
                SELECT  {COUNT}(*),
                        {COUNT}(e.ReportsTo)
                FROM    Employees e
            ";

            AssertIsMatch(sql);
        }
    }
}
