using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using NQuery.Data;
using NQuery.Symbols.Aggregation;
using NQuery.Syntax;
using Xunit;

namespace NQuery.Tests.Legacy
{
    public partial class LegacyTests
    {
        private sealed class FirstOrLastAggregateDefinition : AggregateDefinition
        {
            private readonly bool _isLast;

            public FirstOrLastAggregateDefinition(bool isLast)
            {
                _isLast = isLast;
            }

            public override string Name
            {
                get { return _isLast ? "LAST" : "FIRST"; }
            }

            public override IAggregatable CreateAggregatable(Type argumentType)
            {
                return new FirstOrLastAggregatable(_isLast, argumentType);
            }
        }

        private sealed class FirstOrLastAggregatable : IAggregatable
        {
            private readonly bool _isLast;

            public FirstOrLastAggregatable(bool isLast, Type returnType)
            {
                _isLast = isLast;
                ReturnType = returnType;
            }

            public Type ReturnType { get; }

            public IAggregator CreateAggregator()
            {
                return new FirstOrLastAggregator(_isLast);
            }
        }

        private sealed class FirstOrLastAggregator : IAggregator
        {
            private readonly bool _isLast;
            private object _result;

            public FirstOrLastAggregator(bool isLast)
            {
                _isLast = isLast;
            }

            public void Initialize()
            {
                _result = null;
            }

            public void Accumulate(object value)
            {
                if (_isLast || _result == null)
                    _result = value;
            }

            public object GetResult()
            {
                return _result;
            }
        }

        [Theory]
        [MemberData("GetTestNames")]
        public void LegacyTest_Passes(string name)
        {
            var testDefinitionPath = GetTestDefinitionPath(name);
            var testDefinition = TestDefinition.FromResource(name);
            var dataContext = CreateDataContext();
            var query = Query.Create(dataContext, testDefinition.CommandText);
            var isOrdered = SyntaxTree.ParseQuery(testDefinition.CommandText)
                                      .Root
                                      .DescendantNodes()
                                      .OfType<OrderedQuerySyntax>()
                                      .Any();

            try
            {
                if (testDefinition.ExpectedResults != null)
                {
                    var actualResults = query.ExecuteDataTable();
                    AssertEqual(testDefinition.ExpectedResults, actualResults, isOrdered);
                }
                else if (testDefinition.ExpectedDiagnostics.Any())
                {
                    var actualException = Assert.Throws<CompilationException>(() => query.ExecuteDataTable());
                    //AssertEqual(testDefinition.ExpectedDiagnostics, actualException.Diagnostics);
                }
                else if (testDefinition.ExpectedRuntimeError != null)
                {
                    var actualException = Assert.ThrowsAny<Exception>(() => query.ExecuteDataTable());
                    Assert.Equal(testDefinition.ExpectedRuntimeError, actualException.Message);
                }
            }
            catch (Exception ex)
            {
                var text = "Error in legacy test:" + Environment.NewLine + Environment.NewLine +
                           "   in " + testDefinitionPath;
                throw new Exception(text, ex);
            }
        }

        private static DataContext CreateDataContext()
        {
            var firstAggregate = new AggregateSymbol(new FirstOrLastAggregateDefinition(false));
            var lastAggregate = new AggregateSymbol(new FirstOrLastAggregateDefinition(true));
            var dataContext = NorthwindDataContext.Instance
                .WithJoinTables()
                .AddAggregates(firstAggregate, lastAggregate);
            return dataContext;
        }

        private static string GetTestDefinitionPath(string testName, [CallerFilePath] string sourcePath = null)
        {
            var folderPath = Path.GetDirectoryName(sourcePath);
            var relativePath = testName.Replace(".", "\\") + ".xml";
            return Path.ChangeExtension(folderPath, relativePath);
        }

        private static void AssertEqual(DataTable expectedResults, DataTable actualResults, bool isOrdered)
        {
            if (expectedResults == null)
            {
                Assert.Null(actualResults);
                return;
            }

            Assert.NotNull(actualResults);
            Assert.Equal(expectedResults.Rows.Count, actualResults.Rows.Count);
            Assert.Equal(expectedResults.Columns.Count, actualResults.Columns.Count);

            var expectedRows = GetRows(expectedResults, isOrdered);
            var actualRows = GetRows(actualResults, isOrdered);

            var rows = expectedRows.Zip(actualRows, ValueTuple.Create);
            foreach (var tuple in rows)
            {
                var expectedRow = tuple.Item1;
                var actualRow = tuple.Item2;

                for (var colIndex = 0; colIndex < expectedResults.Columns.Count; colIndex++)
                {
                    var expectedValue = expectedRow[colIndex];
                    var actualValue = actualRow[colIndex];

                    AssertEqual(expectedValue, actualValue);
                }
            }
        }

        private static IEnumerable<DataRow> GetRows(DataTable dataTable, bool isOrdered)
        {
            if (isOrdered)
                return dataTable.Rows.Cast<DataRow>();

            var comparison = new Comparison<DataRow>((x, y) =>
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    var cellX = x[column];
                    var cellY = y[column];

                    var xIsNull = x.IsNull(column);
                    var yIsNull = y.IsNull(column);

                    if (xIsNull && yIsNull)
                        continue;

                    if (xIsNull)
                        return -1;

                    if (yIsNull)
                        return 1;

                    var result = Comparer.Default.Compare(cellX, cellY);
                    if (result != 0)
                        return result;
                }

                return 0;
            });


            var comparer = Comparer<DataRow>.Create(comparison);
            var rows = dataTable.Rows.Cast<DataRow>().ToArray();
            Array.Sort(rows, comparer);

            return rows;
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