using System;
using System.Linq;

using NQuery.Symbols;

using Xunit;

namespace NQuery.Tests.Symbols
{
    public class EumerableTableDefinitionTests
    {
        private sealed class PropertyTableRow
        {
            public PropertyTableRow(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id { get; }
            public string Name { get; }
            private string City { get; set; }
#pragma warning disable 628
            protected string Country { get; set; }
#pragma warning restore 628
            public static string Birthday { get; set; }
        }

        private sealed class FieldTableRow
        {
#pragma warning disable 649
#pragma warning disable 169
            public int Id;
            public string Name;
            private string City;
#pragma warning disable 628
            protected string Country;
#pragma warning restore 628
            public static string Birthday;
#pragma warning restore 169
#pragma warning restore 649
        }

        [Fact]
        public void EumerableTableDefinition_ReturnsPublicNonStaticProperties()
        {
            var rows = Enumerable.Empty<PropertyTableRow>();
            var table = TableDefinition.Create("Table", rows);

            Assert.Equal(2, table.Columns.Length);

            Assert.Equal("Id", table.Columns[0].Name);
            Assert.Equal(typeof(int), table.Columns[0].DataType);

            Assert.Equal("Name", table.Columns[1].Name);
            Assert.Equal(typeof(string), table.Columns[1].DataType);
        }

        [Fact]
        public void EumerableTableDefinition_ReturnsPublicNonStaticFields()
        {
            var rows = Enumerable.Empty<FieldTableRow>();
            var table = TableDefinition.Create("Table", rows);

            Assert.Equal(2, table.Columns.Length);

            Assert.Equal("Id", table.Columns[0].Name);
            Assert.Equal(typeof(int), table.Columns[0].DataType);

            Assert.Equal("Name", table.Columns[1].Name);
            Assert.Equal(typeof(string), table.Columns[1].DataType);
        }

        [Fact]
        public void EumerableTableDefinition_ReturnsObjectValues()
        {
            var rows = new[]
            {
                new PropertyTableRow(1, "Andrew"),
                new PropertyTableRow(1, "Nancy")
            };

            var table = TableDefinition.Create("Table", rows);
            var symbol = new SchemaTableSymbol(table);
            var dataContext = DataContext.Empty.AddTables(symbol);
            var query = Query.Create(dataContext, $"SELECT * FROM {table.Name}");

            using (var reader = query.ExecuteReader())
            {
                Assert.Equal(2, reader.ColumnCount);

                Assert.Equal("Id", reader.GetColumnName(0));
                Assert.Equal(typeof(int), reader.GetColumnType(0));

                Assert.Equal("Name", reader.GetColumnName(1));
                Assert.Equal(typeof(string), reader.GetColumnType(1));

                Assert.True(reader.Read());

                Assert.Equal(rows[0].Id, reader[0]);
                Assert.Equal(rows[0].Name, reader[1]);

                Assert.True(reader.Read());

                Assert.Equal(rows[1].Id, reader[0]);
                Assert.Equal(rows[1].Name, reader[1]);

                Assert.False(reader.Read());
            }
        }
    }
}