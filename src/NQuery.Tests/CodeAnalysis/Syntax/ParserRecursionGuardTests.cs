using NQuery.CodeAnalysis;

namespace NQuery.Tests.CodeAnalysis.Syntax;

public class ParserRecursionGuardTests
{
    [Fact]
    public void Parser_DeeplyNestedExpression_ReportsQueryTooComplexInsteadOfCrashing()
    {
        var text = new string('(', 1000) + "1" + new string(')', 1000);

        var syntaxTree = SyntaxTree.ParseExpression(text);

        Assert.Contains(syntaxTree.GetDiagnostics(), d => d.DiagnosticId == DiagnosticId.QueryTooComplex);
    }

    [Fact]
    public void Parser_DeeplyNestedQuery_ReportsQueryTooComplexInsteadOfCrashing()
    {
        var text = new string('(', 1000) + "SELECT 1" + new string(')', 1000);

        var syntaxTree = SyntaxTree.ParseQuery(text);

        Assert.Contains(syntaxTree.GetDiagnostics(), d => d.DiagnosticId == DiagnosticId.QueryTooComplex);
    }

    [Fact]
    public void Parser_ModeratelyNestedExpression_ParsesWithoutError()
    {
        var text = new string('(', 50) + "1" + new string(')', 50);

        var syntaxTree = SyntaxTree.ParseExpression(text);

        Assert.Empty(syntaxTree.GetDiagnostics());
    }
}
