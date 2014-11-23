using System;

using NQuery.Data;

namespace NQuery.Tests
{
    public static class CompilationExtensions
    {
        public static Compilation WithIdNameTable(this Compilation compilation)
        {
            var table = TestData.IdNameTable();
            return compilation.WithDataContext(DataContext.Default.AddTables(table));
        }

        public static Compilation WithIdNameDataTable(this Compilation compilation)
        {
            var table = TestData.IdNameBytesDataTable();
            return compilation.WithDataContext(DataContext.Default.AddTables(table));
        }
    }
}