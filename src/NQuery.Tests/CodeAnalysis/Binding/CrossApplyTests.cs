using System.Collections.Immutable;
using NQuery.CodeAnalysis;
using NQuery.Northwind;

namespace NQuery.Tests.CodeAnalysis.Binding;

public class CrossApplyTests
{
    [Fact]
    public void CrossApply_Right_CanReferenceLeft()
    {
        // The defining property of APPLY: the right side is evaluated per left row and may
        // reference the left's columns. Binding it puts the left's tables in scope, so the
        // correlated reference resolves without any diagnostics.
        var text = """
            SELECT  e.EmployeeID, oa.OrderID
            FROM    Employees e
                        CROSS APPLY (
                            SELECT  o.OrderID
                            FROM    Orders o
                            WHERE   o.EmployeeID = e.EmployeeID
                        ) oa
            """;

        var diagnostics = GetDiagnostics(text);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void CrossJoin_Right_CannotReferenceLeft()
    {
        // The contrast to the CROSS APPLY case above: a plain CROSS JOIN does not bring the
        // left's tables into scope for the right, so the same correlated reference is an
        // undeclared name. This is what distinguishes APPLY from JOIN at the binder level.
        var text = """
            SELECT  e.EmployeeID, x.OrderID
            FROM    Employees e
                        CROSS JOIN (
                            SELECT  o.OrderID
                            FROM    Orders o
                            WHERE   o.EmployeeID = e.EmployeeID
                        ) x
            """;

        var diagnostics = GetDiagnostics(text);

        Assert.Contains(diagnostics, d => d.DiagnosticId == DiagnosticId.ColumnTableOrVariableNotDeclared);
    }

    private static ImmutableArray<Diagnostic> GetDiagnostics(string text)
    {
        var syntaxTree = SyntaxTree.ParseQuery(text);
        var compilation = Compilation.Empty
                                     .WithCatalog(NorthwindCatalog.Instance)
                                     .WithSyntaxTree(syntaxTree);
        var semanticModel = compilation.GetSemanticModel();
        return syntaxTree.GetDiagnostics().Concat(semanticModel.GetDiagnostics()).ToImmutableArray();
    }
}
