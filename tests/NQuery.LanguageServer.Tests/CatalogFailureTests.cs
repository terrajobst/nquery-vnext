using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.LanguageServer.Infrastructure;

namespace NQuery.LanguageServer;

// When a host's backend is unreachable the editor must say so once, not bury the user in
// unresolved-identifier errors that are really just "there is no catalog".
public sealed class CatalogFailureTests
{
    private const string FailureMessage = @"This is a test error";

    private static readonly Uri DocumentUri = new(@"file:///c:/queries/failure.nql");

    private static Task<LanguageServerTestHarness> StartAsync()
    {
        return LanguageServerTestHarness.StartAsync(catalogProvider: new ThrowingCatalogProvider(FailureMessage));
    }

    [Fact]
    public async Task CatalogFailure_ShowsMessage()
    {
        await using var harness = await StartAsync();

        var message = await harness.ExpectShowMessage().WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        Assert.Equal(MessageType.Error, message.Type);
        Assert.Contains(FailureMessage, message.Message);
    }

    [Fact]
    public async Task CatalogFailure_ReportsOneDiagnosticRatherThanManyUnresolvedNames()
    {
        await using var harness = await StartAsync();

        var expectation = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT c.CompanyName, c.Country FROM Customers c");
        var diagnostics = await expectation;

        // Every name in this query is unresolvable without a catalog. Reporting each one would
        // be noise about a single underlying problem.
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(@"CatalogUnavailable", diagnostic.Code);
        Assert.Contains(FailureMessage, diagnostic.Message);
    }

    [Fact]
    public async Task CatalogFailure_StillReportsSyntaxErrors()
    {
        await using var harness = await StartAsync();

        var expectation = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT FROM WHERE");
        var diagnostics = await expectation;

        // Parsing does not need a catalog, so syntax errors are still worth reporting.
        Assert.Contains(diagnostics, d => d.Code != @"CatalogUnavailable");
        Assert.Contains(diagnostics, d => d.Code == @"CatalogUnavailable");
    }

    [Fact]
    public async Task CatalogFailure_KeepsSyntaxOnlyFeaturesWorking()
    {
        await using var harness = await StartAsync();

        var expectation = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, "SELECT c.CompanyName\nFROM Customers c\nWHERE c.Country = 'Germany'");
        await expectation;

        // Folding and semantic tokens are driven by the syntax tree, so they must survive a
        // missing catalog.
        var ranges = await harness.RequestAsync<FoldingRange[]>(
            Methods.TextDocumentFoldingRange,
            new FoldingRangeParams { TextDocument = new TextDocumentIdentifier { Uri = DocumentUri } });

        Assert.NotEmpty(ranges);

        var tokens = await harness.RequestAsync<SemanticTokens>(
            Methods.TextDocumentSemanticTokensFull,
            new SemanticTokensParams { TextDocument = new TextDocumentIdentifier { Uri = DocumentUri } });

        Assert.NotEmpty(tokens.Data);
    }

    [Fact]
    public async Task CatalogFailure_ExecuteReportsTheCatalogProblem()
    {
        await using var harness = await StartAsync();

        var expectation = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT * FROM Customers");
        await expectation;

        var result = await harness.RequestAsync<ExecuteResult>(
            Methods.NQueryExecute,
            new ExecuteParams { TextDocument = new TextDocumentIdentifier { Uri = DocumentUri } });

        Assert.NotNull(result.ErrorMessage);
        Assert.Contains(FailureMessage, result.ErrorMessage);
    }
}
