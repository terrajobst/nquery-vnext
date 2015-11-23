using System;
using System.Collections.Generic;
using System.Data;

namespace NQuery.Data
{
    public static class QueryReaderExtensions
    {
        public static QueryDataReader ToDataReader(this QueryReader queryReader)
        {
            if (queryReader == null)
                throw new ArgumentNullException(nameof(queryReader));

            return new QueryDataReader(queryReader);
        }

        public static DataTable ExecuteDataTable(this QueryReader queryReader)
        {
            if (queryReader == null)
                throw new ArgumentNullException(nameof(queryReader));

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

        public static DataTable CreateSchemaTable(this QueryReader queryReader)
        {
            if (queryReader == null)
                throw new ArgumentNullException(nameof(queryReader));

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

        public static QueryDataReader ExecuteDataReader(this Query query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return query.ExecuteReader().ToDataReader();
        }

        public static DataTable ExecuteDataTable(this Query query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return query.ExecuteReader().ExecuteDataTable();
        }

        public static DataTable ExecuteSchemaDataTable(this Query query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return query.ExecuteReader().CreateSchemaTable();
        }
    }
}