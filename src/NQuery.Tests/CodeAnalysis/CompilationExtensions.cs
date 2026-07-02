using NQuery.CodeAnalysis;
using NQuery.Data;

namespace NQuery.Tests.CodeAnalysis;

public static class CompilationExtensions
{
    extension(Compilation compilation)
    {
        public Compilation WithIdNameTable()
        {
            var table = TestData.IdNameTable();
            return compilation.WithCatalog(Catalog.Default.AddTables(table));
        }

        public Compilation WithIdNameDataTable()
        {
            var table = TestData.IdNameBytesDataTable();
            return compilation.WithCatalog(Catalog.Default.AddTables(table));
        }
    }
}
