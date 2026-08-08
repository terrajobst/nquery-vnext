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
    public void Parser_DeeplyNestedExpression_TooComplexTreePreservesEntireInput()
    {
        var text = new string('(', 1000) + "1" + new string(')', 1000);

        var syntaxTree = SyntaxTree.ParseExpression(text);

        // Bailing out as "too complex" must still fold the whole input into the tree
        // (as skipped-token trivia) so nothing is dropped -- the root has to span
        // every character, not collapse to an empty/near-empty span.
        Assert.Equal(0, syntaxTree.Root.FullSpan.Start);
        Assert.Equal(text.Length, syntaxTree.Root.FullSpan.End);
    }

    [Fact]
    public void Parser_ModeratelyNestedExpression_ParsesWithoutError()
    {
        var text = new string('(', 50) + "1" + new string(')', 50);

        var syntaxTree = SyntaxTree.ParseExpression(text);

        Assert.Empty(syntaxTree.GetDiagnostics());
    }
}
