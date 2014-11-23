using System;
using System.Data;

namespace NQuery.Tests
{
    public static class TestData
    {
        public static DataTable IdNameTable()
        {
            var table = new DataTable("Table");
            table.Columns.Add(new DataColumn("Id", typeof (int)));
            table.Columns.Add(new DataColumn("Name", typeof (string)));
            table.Rows.Add(1, "One");
            table.Rows.Add(2, "Two");
            return table;
        }

        public static DataTable IdNameBytesDataTable()
        {
            var table = new DataTable("Table");
            table.Columns.Add(new DataColumn("Id", typeof (int)));
            table.Columns.Add(new DataColumn("Name", typeof (string)));
            table.Columns.Add(new DataColumn("Data", typeof (byte[])));
            return table;
        }
    }
}