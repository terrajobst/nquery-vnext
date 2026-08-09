using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.LanguageServer.Infrastructure;
using NQuery.Northwind;

namespace NQuery.LanguageServer;

// The hover fence is colorized by vscode/syntaxes/nquery-quickinfo.tmLanguage.json, whose rules
// anchor on the leading keyword SymbolMarkupBuilder emits for each symbol kind. Nothing at build
// time connects the two: reword a declaration and the grammar simply stops matching, leaving that
// hover unstyled with no error anywhere. These tests are that connection.
public sealed class QuickInfoMarkupTests
{
    private static readonly Uri DocumentUri = new(@"file:///c:/queries/quickinfo.nql");

    private static async Task<string> HoverAtAsync(string text, string target)
    {
        var character = text.IndexOf(target, StringComparison.Ordinal);
        Assert.True(character >= 0, $"'{target}' is not in the query.");

        await using var harness = await LanguageServerTestHarness.StartAsync(NorthwindCatalog.Instance);

        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        var hover = await harness.RequestAsync<Hover?>(
            Methods.TextDocumentHover,
            new HoverParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = DocumentUri },
                Position = new Position { Line = 0, Character = character }
            });

        Assert.NotNull(hover);

        // The declaration line, without the fence the grammar is selected by.
        return hover.Contents.Value
                    .ReplaceLineEndings("\n")
                    .Split('\n')
                    .Single(l => l.Length > 0 && !l.StartsWith("```", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Hover_OnColumn_IsQualifiedAndKeywordLed()
    {
        var declaration = await HoverAtAsync(@"SELECT c.CompanyName FROM Customers c", @"CompanyName");

        // #column keys on COLUMN, and on the '.' to tell the table half from the column half.
        Assert.StartsWith(@"COLUMN c.CompanyName AS ", declaration);
    }

    [Fact]
    public async Task Hover_OnTable_IsKeywordLed()
    {
        var declaration = await HoverAtAsync(@"SELECT c.CompanyName FROM Customers c", @"Customers");

        Assert.StartsWith(@"TABLE Customers", declaration);
    }

    [Fact]
    public async Task Hover_OnTableAlias_IsKeywordLed()
    {
        var declaration = await HoverAtAsync(@"SELECT c.CompanyName FROM Customers c", @"c.");

        // ALIAS names the instance; the nested symbol after FOR is matched by #table.
        Assert.StartsWith(@"ALIAS c FOR TABLE Customers", declaration);
    }

    [Fact]
    public async Task Hover_OnCommonTableExpression_IsKeywordLed()
    {
        const string text = @"WITH Recent AS (SELECT * FROM Orders) SELECT * FROM Recent";

        var declaration = await HoverAtAsync(text, @"Recent AS");

        // Three keywords before the name, which is why #commonTableExpression has to run before
        // #table -- otherwise TABLE would claim EXPRESSION as a table name.
        Assert.StartsWith(@"COMMON TABLE EXPRESSION Recent", declaration);
    }

    [Fact]
    public async Task Hover_OnFunction_HasParameterAndReturnTypes()
    {
        var declaration = await HoverAtAsync(@"SELECT LEN('abc')", @"LEN");

        // #invocable takes the name; every parameter and the return type are '<name> AS <type>',
        // which #parameterInSignature and #type split between them.
        Assert.StartsWith(@"FUNCTION LEN(", declaration);
        Assert.Contains(@" AS ", declaration);
    }
}
