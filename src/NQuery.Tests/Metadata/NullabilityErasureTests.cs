using NQuery.CodeAnalysis;
using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Binding;
using NQuery.CodeAnalysis.Optimization;
using NQuery.CodeAnalysis.Planning;
using NQuery.Northwind;

namespace NQuery.Tests.Metadata;

// Guardrail for the metadata-import erasure of Nullable<T>. The engine models nullability
// separately from the CLR type (SQL NULL is orthogonal, and the row buffer tracks null apart
// from the stored bits), so no type that reaches the type system should ever be Nullable<T>:
// a nullable value-type column must be stored unboxed (e.g. int, not boxed Nullable<int> in the
// object container). These tests fail loudly if a metadata boundary stops erasing.
public class NullabilityErasureTests
{
    // The typed catalog has the nullable value-type columns that exercise erasure:
    // Employees.ReportsTo (int?) and Orders.ShippedDate (DateTime?).
    [Fact]
    public void MetadataImport_ErasesNullableValueTypes_FromColumns()
    {
        var nullable = (from table in NorthwindCatalog.InstanceTyped.Tables
                        from column in table.Columns
                        where column.DataType.IsNullableOfT()
                        select $"{table.Name}.{column.Name}: {column.DataType}").ToList();

        Assert.Empty(nullable);
    }

    [Theory]
    [InlineData("Employees", "ReportsTo", typeof(int))]
    [InlineData("Orders", "ShippedDate", typeof(DateTime))]
    public void MetadataImport_ErasesNullableColumn_ToUnderlyingType(string tableName, string columnName, Type expected)
    {
        var table = NorthwindCatalog.InstanceTyped.Tables.Single(t => t.Name == tableName);
        var column = table.Columns.Single(c => c.Name == columnName);

        Assert.Equal(expected, column.DataType);
    }

    // The strongest boundary check: walk a compiled physical plan over the typed catalog and
    // assert no value slot carries a Nullable<T>. Slot types flow from every metadata boundary
    // (columns, properties, methods, functions), so a missed boundary surfaces here.
    [Theory]
    [InlineData("SELECT EmployeeID, ReportsTo, Region FROM Employees")]
    [InlineData("SELECT OrderID, ShippedDate FROM Orders")]
    [InlineData("SELECT e1.EmployeeID, e2.LastName FROM Employees e1 LEFT JOIN Employees e2 ON e1.ReportsTo = e2.EmployeeID")]
    [InlineData("SELECT ReportsTo, COUNT(*) FROM Employees GROUP BY ReportsTo")]
    [InlineData("SELECT ShippedDate FROM Orders WHERE ShippedDate IS NOT NULL ORDER BY ShippedDate")]
    public void CompiledPlan_HasNoNullableValueTypeSlots(string sql)
    {
        var plan = Plan(sql);

        var nullable = (from op in plan.DescendantsAndSelf()
                        from slot in op.OutputValueSlots
                        where slot.Type.IsNullableOfT()
                        select $"{op.GetType().Name}: {slot.Name} ({slot.Type})").Distinct().ToList();

        Assert.Empty(nullable);
    }

    private static PhysicalOperator Plan(string sql)
    {
        var syntaxTree = SyntaxTree.ParseQuery(sql);
        var bindingResult = Binder.Bind(syntaxTree.Root, NorthwindCatalog.InstanceTyped);
        Assert.Empty(syntaxTree.GetDiagnostics().Concat(bindingResult.Diagnostics));

        var bound = (BoundQuery)bindingResult.BoundRoot;
        var optimized = LogicalOptimizer.Optimize(Algebrizer.Algebrize(bound), NorthwindCatalog.InstanceTyped);
        return Planner.Plan(optimized).Root;
    }
}
