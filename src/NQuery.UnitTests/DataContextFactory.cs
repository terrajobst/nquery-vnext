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
    }
}