using System;

using NQuery.Symbols;

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
            var columns = new[]
                              {
                                  new ColumnSymbol("Id", typeof (int)),
                                  new ColumnSymbol("Name", typeof (string))
                              };
            var table = new SchemaTableSymbol("Table", columns);

            return DataContext.Default.AddTables(table);
        }

        public static DataContext CreateIdNameDataTable()
        {
            var columns = new[]
                              {
                                  new ColumnSymbol("Id", typeof (int)),
                                  new ColumnSymbol("Name", typeof (string)),
                                  new ColumnSymbol("Data", typeof (byte[]))
                              };
            var table = new SchemaTableSymbol("Table", columns);

            return DataContext.Default.AddTables(table);
        }
    }
}