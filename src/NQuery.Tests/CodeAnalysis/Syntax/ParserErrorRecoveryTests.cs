using NQuery.CodeAnalysis;

namespace NQuery.Tests.CodeAnalysis.Syntax;

public class ParserErrorRecoveryTests
{
    [Fact]
    public void Parser_ArgumentList_RecoversWhenAnArgumentCannotBeParsed()
    {
        // '%' cannot start an expression, so parsing the argument consumes no tokens.
        // The argument-list loop has a no-progress guard that skips the offending
        // token; without it the parser would loop forever. Reaching a well-formed
        // tree that spans the whole input proves the guard fired.
        const string text = "SELECT f(%) FROM t";

        var syntaxTree = SyntaxTree.ParseQuery(text);

        Assert.Equal(text.Length, syntaxTree.Root.FullSpan.End);
        Assert.Contains(syntaxTree.GetDiagnostics(), d => d.DiagnosticId == DiagnosticId.TokenExpected);
    }

    [Fact]
    public void Parser_SelectColumnList_RecoversWhenAColumnCannotBeParsed()
    {
        // Same no-progress guard, in the select-column list: '%' can't start a column
        // expression, so the parser must skip it rather than spin in place.
        const string text = "SELECT % FROM t";

        var syntaxTree = SyntaxTree.ParseQuery(text);

        Assert.Equal(text.Length, syntaxTree.Root.FullSpan.End);
        Assert.Contains(syntaxTree.GetDiagnostics(), d => d.DiagnosticId == DiagnosticId.TokenExpected);
    }
}
