using System.Collections.Immutable;
using NQuery.CodeAnalysis;

namespace NQuery.Tests.Binding;

public class OuterApplyTests
{
    [Fact]
    public void OuterApply_Right_CanReferenceLeft()
    {
        // Like CROSS APPLY, the right side of an OUTER APPLY is evaluated per left row and may
        // reference the left's columns. Binding it puts the left's tables in scope, so the
        // correlated reference resolves without any diagnostics.
        var text = @"
                SELECT  e.EmployeeID, oa.OrderID
                FROM    Employees e
                            OUTER APPLY (
                                SELECT  o.OrderID
                                FROM    Orders o
                                WHERE   o.EmployeeID = e.EmployeeID
                            ) oa
            ";

        var diagnostics = GetDiagnostics(text);

        Assert.Empty(diagnostics);
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
