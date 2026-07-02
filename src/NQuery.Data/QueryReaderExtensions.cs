using System.Data;

namespace NQuery.Data;

public static class QueryReaderExtensions
{
    extension(QueryReader queryReader)
    {
        public QueryDataReader ToDataReader()
        {
            ThrowIfNull(queryReader);

            return new QueryDataReader(queryReader);
        }

        public DataTable ExecuteDataTable()
        {
            ThrowIfNull(queryReader);

            var dataTable = queryReader.CreateSchemaTable();
            var values = new object[queryReader.ColumnCount];

            while (queryReader.Read())
            {
                for (var i = 0; i < queryReader.ColumnCount; i++)
                    values[i] = queryReader[i];

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public DataTable CreateSchemaTable()
        {
            ThrowIfNull(queryReader);

            var dataTable = new DataTable(@"Results");
            var existingColumnNames = new HashSet<string>();

            for (var i = 0; i < queryReader.ColumnCount; i++)
            {
                var columnName = queryReader.GetColumnName(i);
                var uniqueColumnName = GenerateUniqueColumnName(existingColumnNames, columnName);
                var columnType = queryReader.GetColumnType(i);
                var dataColumn = new DataColumn(uniqueColumnName, columnType);
                dataColumn.Caption = columnName;
                dataTable.Columns.Add(dataColumn);
                existingColumnNames.Add(uniqueColumnName);
            }

            return dataTable;
        }
    }

    extension(Query query)
    {
        public QueryDataReader ExecuteDataReader()
        {
            ThrowIfNull(query);

            return query.ExecuteReader().ToDataReader();
        }

        public DataTable ExecuteDataTable()
        {
            ThrowIfNull(query);

            return query.ExecuteReader().ExecuteDataTable();
        }

        public DataTable ExecuteSchemaDataTable()
        {
            ThrowIfNull(query);

            return query.ExecuteReader().CreateSchemaTable();
        }
    }

    private static string GenerateUniqueColumnName(HashSet<string> existingColumnNames, string columnName)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            columnName = @"Column";

        var result = columnName;
        var count = 0;
        while (existingColumnNames.Contains(result))
            result = string.Concat(columnName, ++count);

        return result;
    }
}
