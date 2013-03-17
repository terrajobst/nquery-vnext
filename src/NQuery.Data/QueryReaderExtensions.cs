using System;
using System.Data;

namespace NQuery.Data
{
    public static class QueryReaderExtensions
    {
        public static QueryDataReader ToDataReader(this QueryReader queryReader)
        {
            if (queryReader == null)
                throw new ArgumentNullException("queryReader");

            return new QueryDataReader(queryReader);
        }

        public static DataTable ExecuteDataTable(this QueryReader queryReader)
        {
            if (queryReader == null)
                throw new ArgumentNullException("queryReader");

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
                throw new ArgumentNullException("queryReader");

            var dataTable = new DataTable("Results");

            for (var i = 0; i < queryReader.ColumnCount; i++)
            {
                var columnName = queryReader.GetColumnName(i);
                var columnType = queryReader.GetColumnType(i);
                var dataColumn = new DataColumn(columnName, columnType);
                dataTable.Columns.Add(dataColumn);
            }

            return dataTable;
        }

        public static QueryDataReader ExecuteDataReader(this Query query)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            return query.ExecuteReader().ToDataReader();
        }

        public static DataTable ExecuteDataTable(this Query query)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            return query.ExecuteReader().ExecuteDataTable();
        }

        public static DataTable ExecuteSchemaDataTable(this Query query)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            return query.ExecuteReader().CreateSchemaTable();
        }
    }
}