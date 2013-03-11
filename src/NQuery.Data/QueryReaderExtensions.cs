using System;
using System.Data;

namespace NQuery.Data
{
    public static class QueryReaderExtensions
    {
        public static DataTable ExecuteDataTable(this QueryReader queryReader)
        {
            var dataTable = new DataTable("Results");

            for (var i = 0; i < queryReader.ColumnCount; i++)
            {
                var columnName = queryReader.GetColumnName(i);
                var columnType = queryReader.GetColumnType(i);
                var dataColumn = new DataColumn(columnName, columnType);
                dataTable.Columns.Add(dataColumn);
            }

            var values = new object[queryReader.ColumnCount];

            while (queryReader.Read())
            {
                for (var i = 0; i < queryReader.ColumnCount; i++)
                    values[i] = queryReader[i];

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
    }
}