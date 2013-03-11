using System;
using System.Data;

using NQuery.Data;

namespace NQuery.UnitTests
{
    public static class DataContextFactory
    {
        public static Compilation WithIdNameTable(this Compilation compilation)
        {
            return compilation.WithDataContext(CreateIdNameTable());
        }

        public static Compilation WithIdNameDataTable(this Compilation compilation)
        {
            return compilation.WithDataContext(CreateIdNameDataTable());
        }

        public static DataContext CreateIdNameTable()
        {
            var table = new DataTable("Table");
            table.Columns.Add(new DataColumn("Id", typeof (int)));
            table.Columns.Add(new DataColumn("Name", typeof (string)));
            return DataContext.Default.AddTables(table);
        }

        public static DataContext CreateIdNameDataTable()
        {
            var table = new DataTable("Table");
            table.Columns.Add(new DataColumn("Id", typeof(int)));
            table.Columns.Add(new DataColumn("Name", typeof(string)));
            table.Columns.Add(new DataColumn("Data", typeof(byte[])));
            return DataContext.Default.AddTables(table);
        }
    }
}