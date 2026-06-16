using NQuery.CodeAnalysis;
using NQuery.Data;

namespace NQuery.Tests.CodeAnalysis;

public static class CompilationExtensions
{
    public static Compilation WithIdNameTable(this Compilation compilation)
    {
        var table = TestData.IdNameTable();
        return compilation.WithCatalog(Catalog.Default.AddTables(table));
    }

    public static Compilation WithIdNameDataTable(this Compilation compilation)
    {
        var table = TestData.IdNameBytesDataTable();
        return compilation.WithCatalog(Catalog.Default.AddTables(table));
    }
}
