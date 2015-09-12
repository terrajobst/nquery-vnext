using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using NQuery.Authoring.Renaming;

using Xunit;

namespace NQuery.Authoring.Tests.Renaming
{
    public class RenameModelTests
    {
        private static Task AssertIsMatch(string queryWithChangeMarkers)
        {
            return PerformAssert(queryWithChangeMarkers, AssertIsMatch);
        }

        private static Task AssertIsNoMatch(string queryWithChangeMarkers)
        {
            return PerformAssert(queryWithChangeMarkers, AssertIsNoMatch);
        }

        private static async Task PerformAssert(string queryWithChangeMarkers, Func<Document, int, string, Task> asserter)
        {
            var annotatedText = AnnotatedText.Parse(queryWithChangeMarkers.NormalizeCode());

            Assert.Single(annotatedText.Changes);
            Assert.NotEmpty(annotatedText.Spans);

            var renameChange = annotatedText.Changes.Single();
            var expectedChangeSpans = annotatedText.Spans.Add(renameChange.Span)
                                                         .OrderByDescending(s => s.Start)
                                                         .ToImmutableArray();

            var query = annotatedText.Text;

            var newName = renameChange.NewText;

            var compilation = CompilationFactory.CreateQuery(query);
            var document = new Document(DocumentKind.Query, compilation.DataContext, compilation.SyntaxTree.Text);

            foreach (var expectedChangeSpan in expectedChangeSpans)
            {
                await asserter(document, expectedChangeSpan.Start, newName);
                await asserter(document, expectedChangeSpan.End, newName);
            }
        }

        private static async Task AssertIsMatch(Document document, int position, string newName)
        {
            var renameModel = await RenameModel.CreateAsync(document, position);

            Assert.True(renameModel.CanRename);

            var renamedDocument = await renameModel.RenameAsync(newName);

            foreach (var location in renamedDocument.Locations)
            {
                var renamedText = renamedDocument.Document.Text.GetText(location);
                Assert.Equal(newName, renamedText);
            }
        }

        private static async Task AssertIsNoMatch(Document document, int position, string newName)
        {
            var renameModel = await RenameModel.CreateAsync(document, position);

            Assert.False(renameModel.CanRename);
        }

        [Fact]
        public Task RenameModel_CanRename_TableInstance()
        {
            var query = @"
                SELECT  {e}.FirstName
                FROM    Employees {e:emp}
            ";

            return AssertIsMatch(query);
        }

        [Fact]
        public Task RenameModel_CanRename_QueryColumnInstance()
        {
            var query = @"
                SELECT  e.FirstName + ' ' + e.LastName AS {FullName}
                FROM    Employees e
                ORDER   BY {FullName:Employee}
            ";

            return AssertIsMatch(query);
        }

        [Fact]
        public Task RenameModel_CanRename_DerivedTable()
        {
            var query = @"
                SELECT  {d}.FirstName,
                        {d}.LastName
                FROM    (   SELECT  *
                            FROM    Employees e
                            WHERE   e.City = 'London'  ) {d:londonEmps}
            ";

            return AssertIsMatch(query);
        }

        [Fact]
        public Task RenameModel_CanRename_CommonTableExpression()
        {
            var query = @"
                WITH {le:LondonEmps} AS
                (
                    SELECT  *
                    FROM    Employees e
                    WHERE   e.City = 'London'
                )
                SELECT  *
                FROM    {le} l1
                            INNER JOIN {le} l2 ON l1.EmployeeID = l2.ReportsTo
            ";

            return AssertIsMatch(query);
        }

        [Fact]
        public Task RenameModel_CannotRename_Table()
        {
            var query = @"
                SELECT  *
                FROM    {Employees:e} e1
                            INNER JOIN {Employees} e2 ON e1.EmployeeId = e2.EmployeeId
            ";

            return AssertIsNoMatch(query);
        }

        [Fact]
        public Task RenameModel_CannotRename_TableColumnInstance()
        {
            var query = @"
                SELECT  e.{FirstName:f}
                FROM    Employees e
                WHERE   e.{FirstName} = 'Andrew'
            ";

            return AssertIsNoMatch(query);
        }

        [Fact]
        public Task RenameModel_CannotRename_Function()
        {
            var query = @"
                SELECT  {TO_STRING:t}(e.EmployeeId),
                        {TO_STRING}(e.BirthDate)
                FROM    Employees e
            ";

            return AssertIsNoMatch(query);
        }

        [Fact]
        public Task RenameModel_CannotRename_Aggregate()
        {
            var query = @"
                SELECT  {MAX:m}(e.EmployeeId),
                        {MAX}(e.BirthDate)
                FROM    Employees e
            ";

            return AssertIsNoMatch(query);
        }

        [Fact]
        public Task RenameModel_CannotRename_Property()
        {
            var query = @"
                SELECT  e.FirstName.{Length:l},
                        e.LastName.{Length}
                FROM    Employees e
            ";

            return AssertIsNoMatch(query);
        }

        [Fact]
        public Task RenameModel_CannotRename_Method()
        {
            var query = @"
                SELECT  e.EmployeeId.{ToString:t}(),
                        e.ReportsTo.{ToString}()
                FROM    Employees e
            ";

            return AssertIsNoMatch(query);
        }
    }
}