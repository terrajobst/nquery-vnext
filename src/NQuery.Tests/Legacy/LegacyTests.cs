using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;

using NQuery.Data;

using Xunit;

namespace NQuery.Tests.Legacy
{
    public partial class LegacyTests
    {
        [Theory]
        [MemberData("GetTestNames")]
        public void LegacyTest_Passes(string name)
        {
            var testDefinition = TestDefinition.FromResource(name);
            var dataContext = NorthwindDataContext.Instance.WithJoinTables();
            var query = Query.Create(dataContext, testDefinition.CommandText);

            if (testDefinition.ExpectedResults != null)
            {
                var actualResults = query.ExecuteDataTable();
                AssertEqual(testDefinition.ExpectedResults, actualResults);
            }
            else if (testDefinition.ExpectedDiagnostics.Any())
            {
                var actualException = Assert.Throws<CompilationException>(() => query.ExecuteDataTable());
                AssertEqual(testDefinition.ExpectedDiagnostics, actualException.Diagnostics);
            }
            else if (testDefinition.ExpectedRuntimeError != null)
            {
                var actualException = Assert.Throws<Exception>(() => query.ExecuteDataTable());
                Assert.Equal(testDefinition.ExpectedRuntimeError, actualException.Message);
            }
        }

        private static void AssertEqual(DataTable expectedResults, DataTable actualResults)
        {
            if (expectedResults == null)
            {
                Assert.Null(actualResults);
                return;
            }

            Assert.NotNull(actualResults);
            Assert.Equal(expectedResults.Rows.Count, actualResults.Rows.Count);
            Assert.Equal(expectedResults.Columns.Count, actualResults.Columns.Count);

            for (var rowIndex = 0; rowIndex < expectedResults.Rows.Count; rowIndex++)
            {
                var expectedRow = expectedResults.Rows[rowIndex];
                var actualRow = actualResults.Rows[rowIndex];

                for (var colIndex = 0; colIndex < expectedResults.Columns.Count; colIndex++)
                {
                    var expectedValue = expectedRow[colIndex];
                    var actualValue = actualRow[colIndex];

                    AssertEqual(expectedValue, actualValue);
                }
            }
        }

        private static void AssertEqual(object expectedValue, object actualValue)
        {
            var expectedBytes = expectedValue as byte[];
            var actualBytes = actualValue as byte[];
            if (expectedBytes != null && actualBytes != null)
                AssertEqual(expectedBytes, actualBytes);
            else
                Assert.Equal(expectedValue, actualValue);
        }

        public static void AssertEqual(byte[] expectedValue, byte[] actualValue)
        {
            var expectedBase64String = Convert.ToBase64String(expectedValue);
            var actualBase64String = Convert.ToBase64String(actualValue);
            Assert.Equal(expectedBase64String, actualBase64String);
        }

        private static void AssertEqual(ImmutableArray<Diagnostic> expected, ImmutableArray<Diagnostic> actual)
        {
            Assert.Equal(expected.Length, actual.Length);

            for (var i = 0; i < expected.Length; i++)
            {
                //Assert.Equal(expected[i].Span, actual[i].Span);
                Assert.Equal(expected[i].DiagnosticId, actual[i].DiagnosticId);
                Assert.Equal(expected[i].Message, actual[i].Message);
            }
        }

        public static IEnumerable<object[]> GetTestNames()
        {
            return TestDefinition.GetResourceNames()
                                 .Select(n => new object[] { n });
        }
    }
}