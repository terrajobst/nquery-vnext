using System.Collections.Generic;
using System.Threading.Tasks;
using NQuery.Authoring.Renaming;
using NQuery.Text;
using Xunit;

namespace NQuery.Authoring.Tests.Renaming
{
    public class RenameTests
    {
        private static Task<RenamedDocument> Create(string queryWithEdit)
        {
            var annotatedText = AnnotatedText.Parse(queryWithEdit);
            Assert.Empty(annotatedText.Spans);
            var change = Assert.Single(annotatedText.Changes);
            var query = annotatedText.Text;

            var text = SourceText.From(query);
            var workspace = new Workspace(text.Container)
            {
                DataContext = NorthwindDataContext.Instance,
                DocumentKind = DocumentKind.Query
            };
            return RenamedDocument.CreateAsync(workspace.CurrentDocument, change);
        }

        private async Task AssertIsMatch(string queryWithEdit, string expectedQueryWithSpans)
        {
            var result = await Create(queryWithEdit.NormalizeCode());
            var expectedQuery = expectedQueryWithSpans.NormalizeCode().ParseSpans(out var expectedSpans);

            Assert.True(result.IsRenamed);
            Assert.Equal(expectedQuery, result.NewDocument.Text.GetText());
            Assert.Equal((IEnumerable<TextSpan>)expectedSpans, result.NewSpans);
        }

        private async Task AssertIsInvalid(string queryWithEdit)
        {
            var result = await Create(queryWithEdit.NormalizeCode());
            Assert.False(result.IsRenamed);
        }

        [Fact]
        public async Task RenamedDocument_DoesNotRename_WhenInWhitespace()
        {
            var query = @"
                SELECT  e.EmployeeID,{: -- Added comment}
                        e.LastName
                FROM    Employees e
            ";

            await AssertIsInvalid(query);
        }

        [Fact]
        public async Task RenamedDocument_DoesNotRename_WhenAddingComma()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.LastName{:,}
                FROM    Employees e
            ";

            await AssertIsInvalid(query);
        }

        [Fact]
        public async Task RenamedDocument_DoesNotRename_WhenInsertingWhitespace()
        {
            var query = @"
                SELECT  emp.EmployeeID,
                        emp.LastName
                FROM    Employees em{: }p
            ";

            await AssertIsInvalid(query);
        }

        [Fact]
        public async Task RenamedDocument_Renames_WhenPrefixing()
        {
            var query = @"
                SELECT  p.EmployeeID,
                        p.LastName
                FROM    Employees {:em}p
            ";

            var expected = @"
                SELECT  {emp}.EmployeeID,
                        {emp}.LastName
                FROM    Employees {emp}
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact]
        public async Task RenamedDocument_Renames_WhenInserting()
        {
            var query = @"
                SELECT  ep.EmployeeID,
                        ep.LastName
                FROM    Employees e{:m}p
            ";

            var expected = @"
                SELECT  {emp}.EmployeeID,
                        {emp}.LastName
                FROM    Employees {emp}
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact]
        public async Task RenamedDocument_Renames_WhenReplacing()
        {
            var query = @"
                SELECT  x.EmployeeID,
                        x.LastName
                FROM    Employees {x:emp}
            ";

            var expected = @"
                SELECT  {emp}.EmployeeID,
                        {emp}.LastName
                FROM    Employees {emp}
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact]
        public async Task RenamedDocument_Renames_WhenSuffixing()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.LastName
                FROM    Employees e{:mp}
            ";

            var expected = @"
                SELECT  {emp}.EmployeeID,
                        {emp}.LastName
                FROM    Employees {emp}
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact]
        public async Task RenamedDocument_Renames_WhenDeletingAtStart()
        {
            var query = @"
                SELECT  emp.EmployeeID,
                        emp.LastName
                FROM    Employees {em:}p
            ";

            var expected = @"
                SELECT  {p}.EmployeeID,
                        {p}.LastName
                FROM    Employees {p}
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact]
        public async Task RenamedDocument_Renames_WhenDeletingAtEnd()
        {
            var query = @"
                SELECT  emp.EmployeeID,
                        emp.LastName
                FROM    Employees e{mp:}
            ";

            var expected = @"
                SELECT  {e}.EmployeeID,
                        {e}.LastName
                FROM    Employees {e}
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact]
        public async Task RenamedDocument_DoesNotRename_Table()
        {
            var query = @"
                SELECT  manager.LastName,
                        report.LastName
                FROM    Employ{ees:xx} manager
                            INNER JOIN Employees report ON report.ReportsTo = manager.EmployeeId
            ";

            await AssertIsInvalid(query);
        }

        [Fact]
        public async Task RenamedDocument_DoesNotRename_Column()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.Employee{ID:XX}
                FROM    Employees e
            ";

            await AssertIsInvalid(query);
        }

        [Fact]
        public async Task RenamedDocument_Renames_TableInstance()
        {
            var query = @"
                SELECT  e.EmployeeID,
                        e.LastName
                FROM    Employees e{:2}
            ";

            var expected = @"
                SELECT  {e2}.EmployeeID,
                        {e2}.LastName
                FROM    Employees {e2}
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact]
        public async Task RenamedDocument_Renames_CommonTableExpression()
        {
            var query = @"
                WITH LondonEmps AS
                (
                    SELECT  *
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  *
                FROM    LondonEmps{:2} l
            ";

            var expected = @"
                WITH {LondonEmps2} AS
                (
                    SELECT  *
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  *
                FROM    {LondonEmps2} l
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact]
        public async Task RenamedDocument_Renames_CommonTableExpression_WhenRecursive()
        {
            var query = @"
                WITH RECURSIVE EmpHierarchy AS
                (
                    SELECT  e.EmployeeID,
                            e.FirstName,
                            e.LastName,
                            0 AS Level,
                            e.EmployeeID.ToString() AS Path
                    FROM    Employees e
                    WHERE   e.ReportsTo IS NULL

                    UNION ALL

                    SELECT  e.EmployeeID,
                            e.FirstName,
                            e.LastName,
                            eh.Level + 1,
                            eh.Path + '.' + e.EmployeeID
                    FROM    Employees e
                                INNER JOIN EmpHierarchy eh ON eh.EmployeeID = e.ReportsTo
                )
                SELECT  SPACE(8 * e.Level) + e.FirstName + ' ' + e.LastName
                FROM    EmpHierarchy{:2} e
                ORDER   BY e.Path
            ";

            var expected = @"
                WITH RECURSIVE {EmpHierarchy2} AS
                (
                    SELECT  e.EmployeeID,
                            e.FirstName,
                            e.LastName,
                            0 AS Level,
                            e.EmployeeID.ToString() AS Path
                    FROM    Employees e
                    WHERE   e.ReportsTo IS NULL

                    UNION ALL

                    SELECT  e.EmployeeID,
                            e.FirstName,
                            e.LastName,
                            eh.Level + 1,
                            eh.Path + '.' + e.EmployeeID
                    FROM    Employees e
                                INNER JOIN {EmpHierarchy2} eh ON eh.EmployeeID = e.ReportsTo
                )
                SELECT  SPACE(8 * e.Level) + e.FirstName + ' ' + e.LastName
                FROM    {EmpHierarchy2} e
                ORDER   BY e.Path
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact]
        public async Task RenamedDocument_Renames_ColumnInstance()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName AS FullName
                FROM    Employees e
                ORDER   BY FullName{:2}
            ";

            var expected = @"
                SELECT  e.FirstName + ' ' + e.LastName AS {FullName2}
                FROM    Employees e
                ORDER   BY {FullName2}
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact(Skip = "Not implemented yet")]
        public async Task RenamedDocument_Renames_CommonTableExpression_ExplicitColumn()
        {
            var query = @"
                WITH LondonEmps (EmployeeID, FirstName, LastName) AS
                (
                    SELECT  EmployeeID, FirstName, LastName
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  l.First{Name:}
                FROM    LondonEmps l
            ";

            var expected = @"
                WITH LondonEmps (EmployeeID, {First}, LastName) AS
                (
                    SELECT  EmployeeID, FirstName, LastName
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  l.{First}
                FROM    LondonEmps l
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact(Skip = "Not implemented yet")]
        public async Task RenamedDocument_Renames_CommonTableExpression_ImplicitColumn()
        {
            var query = @"
                WITH LondonEmps AS
                (
                    SELECT  EmployeeID, FirstName AS FirstName, LastName
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  l.First{Name:}
                FROM    LondonEmps l
            ";

            var expected = @"
                WITH LondonEmps AS
                (
                    SELECT  EmployeeID, FirstName AS {First}, LastName
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  l.{First}
                FROM    LondonEmps l
            ";

            await AssertIsMatch(query, expected);
        }

        [Fact(Skip = "Not implemented yet")]
        public async Task RenamedDocument_Renames_DerivedTableColumn()
        {
            var query = @"
                SELECT  l.FullName
                FROM    (
                            SELECT  e.FirstName + ' ' + e.LastName AS FullName{:2}
                            FROM    Employees e
                            WHERE   e.City = 'London'
                        ) l
            ";

            var expected = @"
                SELECT  l.{FullName2}
                FROM    (
                            SELECT  e.FirstName + ' ' + e.LastName AS {FullName2}
                            FROM    Employees e
                            WHERE   e.City = 'London'
                        ) l
            ";

            await AssertIsMatch(query, expected);
        }
    }
}
