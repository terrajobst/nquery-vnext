using System;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Text;

using Xunit;

namespace NQuery.Tests.Binding
{
    public class TableRelationTests
    {
        private static void AssertHasErrors(string query)
        {
            ImmutableArray<TextSpan> spans;
            var compilation = CompilationFactory.CreateQuery(query, out spans);

            var semanticModel = compilation.GetSemanticModel();
            var syntaxTree = semanticModel.SyntaxTree;
            var diagnostics = syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();

            Assert.Equal(spans.Length, diagnostics.Length);

            for (var i = 0; i < spans.Length; i++)
            {
                var span = spans[i];
                var diagnostic = diagnostics[i];

                Assert.Equal(span, diagnostic.Span);
                Assert.Equal(DiagnosticId.DuplicateTableRefInFrom, diagnostic.DiagnosticId);
            }
        }

        [Fact]
        public void TableRelation_DuplicateAliases_Between_NamedTables_IfUnnamed()
        {
            var query = @"
                SELECT  *
                FROM    {Employees},
                        {Employees}
            ";

            AssertHasErrors(query);
        }

        [Fact]
        public void TableRelation_DuplicateAliases_Between_NamedTables_IfNamed()
        {
            var query = @"
                SELECT  *
                FROM    Employees {e},
                        EmployeeTerritories AS {e}
            ";

            AssertHasErrors(query);
        }

        [Fact]
        public void TableRelation_DuplicateAliases_Between_DerivedTables()
        {
            var query = @"
                SELECT  *
                FROM    (SELECT * FROM Employees) {e},
                        (SELECT * FROM EmployeeTerritories) AS {e}
            ";

            AssertHasErrors(query);
        }

        [Fact]
        public void TableRelation_DuplicateAliases_Between_NamedAndDerivedTable()
        {
            var query = @"
                SELECT  *
                FROM    Employees AS {e},
                        (SELECT * FROM EmployeeTerritories) {e}
            ";

            AssertHasErrors(query);
        }

        [Fact]
        public void TableRelation_DuplicateAliases_Between_NamedTables_InJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees AS {e}
                            INNER JOIN Employees AS {e} ON TRUE
            ";

            AssertHasErrors(query);
        }

        [Fact]
        public void TableRelation_DuplicateAliases_Between_NamedTables_InDifferentJoin()
        {
            var query = @"
                SELECT  *
                FROM    Employees AS e
                            JOIN EmployeeTerritories AS {t} ON TRUE,
                        Region r
                            JOIN Territories {t} ON TRUE
            ";

            AssertHasErrors(query);
        }
    }
}