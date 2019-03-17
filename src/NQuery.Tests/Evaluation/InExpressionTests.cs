using System;
using Xunit;

namespace NQuery.Tests.Evaluation
{
    public class InExpressionTests
    {
        private void AssertProduces<T>(string text, T[] expected)
        {
            var expectedColumns = new[] { typeof(T) };
            var expectedRows = new object[expected.Length][];

            for (var i = 0; i < expected.Length; i++)
                expectedRows[i] = new object[] { expected[i] };

            AssertProduces(text, expectedColumns, expectedRows);
        }

        private void AssertProduces<T1, T2>(string text, ValueTuple<T1, T2>[] expected)
        {
            var expectedColumns = new[] { typeof(T1), typeof(T2) };
            var expectedRows = new object[expected.Length][];

            for (var i = 0; i < expected.Length; i++)
                expectedRows[i] = new object[] { expected[i].Item1, expected[i].Item2 };

            AssertProduces(text, expectedColumns, expectedRows);
        }

        private void AssertProduces(string text, Type[] expectedColumns, object[][] expectedRows)
        {
            var dataContext = NorthwindDataContext.Instance;
            var query = Query.Create(dataContext, text);
            using (var data = query.ExecuteReader())
            {
                Assert.Equal(expectedColumns.Length, data.ColumnCount);

                for (var i = 0; i < expectedColumns.Length; i++)
                    Assert.Equal(expectedColumns[i], data.GetColumnType(i));

                var rowIndex = 0;

                while (data.Read())
                {
                    var expectedRow = expectedRows[rowIndex];

                    for (var columnIndex = 0; columnIndex < expectedColumns.Length; columnIndex++)
                        Assert.Equal(expectedRow[columnIndex], data[columnIndex]);

                    rowIndex++;
                }
            }
        }

        [Fact]
        public void Evaluation_InExpression()
        {
            var text = @"
                SELECT  e.EmployeeID
                FROM    Employees e
                WHERE   e.EmployeeID NOT IN (1, 2, 3)
            ";

            var expected = new[] { 4, 5, 6, 7, 8, 9 };

            AssertProduces(text, expected);
        }
    }
}
