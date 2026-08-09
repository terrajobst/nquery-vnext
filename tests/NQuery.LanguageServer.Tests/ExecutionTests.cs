using NQuery.Authoring.LanguageServer;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.LanguageServer.Infrastructure;
using NQuery.Northwind;

namespace NQuery.LanguageServer;

public sealed class ExecutionTests
{
    private static readonly Uri QueryUri = new(@"file:///c:/queries/run.nql");
    private static readonly Uri ExpressionUri = new(@"file:///c:/queries/run.nqe");

    private static Task<LanguageServerTestHarness> StartAsync(Action<NQueryLanguageServerOptions>? configure = null)
    {
        return LanguageServerTestHarness.StartAsync(NorthwindCatalog.Instance, configure: configure);
    }

    private static async Task<LanguageServerTestHarness> OpenAsync(Uri uri, string text, Action<NQueryLanguageServerOptions>? configure = null)
    {
        var harness = await StartAsync(configure);
        var opened = harness.ExpectDiagnostics(uri);
        await harness.OpenAsync(uri, text);
        await opened;
        return harness;
    }

    private static Task<ExecuteResult> ExecuteAsync(LanguageServerTestHarness harness, Uri uri, int? maxRows = null)
    {
        return harness.RequestAsync<ExecuteResult>(
            Methods.NQueryExecute,
            new ExecuteParams { TextDocument = new TextDocumentIdentifier { Uri = uri }, MaxRows = maxRows });
    }

    [Fact]
    public async Task Execute_ReturnsColumnsAndRows()
    {
        await using var harness = await OpenAsync(QueryUri,
            @"SELECT c.CompanyName, c.Country FROM Customers c WHERE c.Country = 'Germany'");

        var result = await ExecuteAsync(harness, QueryUri);

        Assert.Null(result.ErrorMessage);
        Assert.Equal(2, result.Columns.Count);
        Assert.Equal(@"CompanyName", result.Columns[0].Name);
        Assert.Equal(@"string", result.Columns[0].Type);
        Assert.NotEmpty(result.Rows);
        Assert.All(result.Rows, r => Assert.Equal(@"Germany", r[1]));
        Assert.False(result.Truncated);
    }

    [Fact]
    public async Task Execute_RendersSqlNullAsJsonNull()
    {
        await using var harness = await OpenAsync(QueryUri,
            @"SELECT o.OrderID, o.ShippedDate FROM Orders o WHERE o.ShippedDate IS NULL");

        var result = await ExecuteAsync(harness, QueryUri);

        Assert.Null(result.ErrorMessage);
        Assert.NotEmpty(result.Rows);

        // NULL is JSON null, which is what distinguishes it from the literal text "NULL".
        Assert.All(result.Rows, r => Assert.Null(r[1]));
    }

    [Fact]
    public async Task Execute_RendersBinaryAsPlaceholder()
    {
        // Categories.Picture holds real image data; serializing it would be megabytes of base64
        // that the grid could not display anyway.
        await using var harness = await OpenAsync(QueryUri, @"SELECT c.CategoryName, c.Picture FROM Categories c");

        var result = await ExecuteAsync(harness, QueryUri);

        Assert.Null(result.ErrorMessage);
        Assert.Equal(@"byte[]", result.Columns[1].Type);
        Assert.All(result.Rows, r => Assert.StartsWith(@"byte[", r[1]));
    }

    [Fact]
    public async Task Execute_IsUncappedByDefault()
    {
        // The client pages through the result rather than rendering all of it, so the default is
        // to hand over every row. [Order Details] has more rows than the old 1000-row cap.
        await using var harness = await OpenAsync(QueryUri, @"SELECT d.OrderID FROM [Order Details] d");

        var result = await ExecuteAsync(harness, QueryUri);

        Assert.True(result.Rows.Count > 1000, $"Rows: {result.Rows.Count}");
        Assert.False(result.Truncated);
    }

    [Fact]
    public async Task Execute_CapsRowsAndReportsTruncation()
    {
        await using var harness = await OpenAsync(QueryUri, @"SELECT o.OrderID FROM Orders o");

        var result = await ExecuteAsync(harness, QueryUri, maxRows: 5);

        Assert.Equal(5, result.Rows.Count);
        Assert.True(result.Truncated);
    }

    [Fact]
    public async Task Execute_ClientCannotRaiseTheServerCap()
    {
        await using var harness = await OpenAsync(QueryUri, @"SELECT o.OrderID FROM Orders o",
            options => options.MaxRows = 3);

        var result = await ExecuteAsync(harness, QueryUri, maxRows: 10_000);

        Assert.Equal(3, result.Rows.Count);
        Assert.True(result.Truncated);
    }

    [Fact]
    public async Task Execute_EvaluatesExpressionDocument()
    {
        // A .nqe document binds to a bare expression; the algebrizer wraps it in a one-row
        // projection, so it runs through the same path as a query.
        await using var harness = await OpenAsync(ExpressionUri, @"COALESCE(NULL, 40) + 2");

        var result = await ExecuteAsync(harness, ExpressionUri);

        Assert.Null(result.ErrorMessage);
        var row = Assert.Single(result.Rows);
        Assert.Equal(@"42", Assert.Single(row));
    }

    [Fact]
    public async Task Execute_WithCompilationErrors_ReportsMessage()
    {
        await using var harness = await OpenAsync(QueryUri, @"SELECT * FROM Bogus");

        var result = await ExecuteAsync(harness, QueryUri);

        Assert.NotNull(result.ErrorMessage);
        Assert.Contains(@"Bogus", result.ErrorMessage);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public async Task Execute_WhenDisabled_ReportsMessage()
    {
        await using var harness = await OpenAsync(QueryUri, @"SELECT * FROM Customers",
            options => options.AllowExecution = false);

        var result = await ExecuteAsync(harness, QueryUri);

        Assert.NotNull(result.ErrorMessage);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public async Task Initialize_AdvertisesExecutionCapability()
    {
        await using var enabled = await StartAsync();
        Assert.True(enabled.InitializeResult.Capabilities.Experimental?.Execute);
        Assert.True(enabled.InitializeResult.Capabilities.Experimental?.ShowPlan);

        await using var disabled = await StartAsync(options => options.AllowExecution = false);
        Assert.False(disabled.InitializeResult.Capabilities.Experimental?.Execute);
    }

    [Fact]
    public async Task ShowPlan_ReturnsThePipeline()
    {
        await using var harness = await OpenAsync(QueryUri,
            @"SELECT c.CompanyName FROM Customers c INNER JOIN Orders o ON o.CustomerID = c.CustomerID");

        var result = await harness.RequestAsync<ShowPlanResult>(
            Methods.NQueryShowPlan,
            new ShowPlanParams { TextDocument = new TextDocumentIdentifier { Uri = QueryUri } });

        Assert.Null(result.ErrorMessage);

        // The unoptimized logical tree, the passes that changed it, the optimized tree, and the
        // physical plan -- so there is always more than one step.
        Assert.True(result.Steps.Count >= 2, $"Steps: {string.Join(@", ", result.Steps.Select(s => s.Name))}");

        var last = result.Steps[^1];
        Assert.NotEmpty(last.Root.OperatorName);
        Assert.NotEmpty(last.Root.Children);
    }

    [Fact]
    public async Task ShowPlan_DescribesOperatorsAndMarksScalars()
    {
        await using var harness = await OpenAsync(QueryUri, @"SELECT c.CompanyName FROM Customers c WHERE c.Country = 'Germany'");

        var result = await harness.RequestAsync<ShowPlanResult>(
            Methods.NQueryShowPlan,
            new ShowPlanParams { TextDocument = new TextDocumentIdentifier { Uri = QueryUri } });

        var nodes = Flatten(result.Steps[^1].Root).ToArray();

        // Detail lives in OperatorName, not in Properties -- ShowPlanNode.Properties is empty for
        // most operators, so the tree has to lead with the name.
        Assert.All(nodes, n => Assert.NotEmpty(n.OperatorName));
        Assert.Contains(nodes, n => n.OperatorName.StartsWith(@"Table (Customers)", StringComparison.Ordinal));

        // Scalar subtrees (the WHERE comparison) are flagged so the client can render them
        // differently from relational operators.
        Assert.Contains(nodes, n => n.IsScalar);
        Assert.Contains(nodes, n => !n.IsScalar);
    }

    [Fact]
    public async Task ShowPlan_WithErrors_ReportsMessage()
    {
        await using var harness = await OpenAsync(QueryUri, @"SELECT * FROM Bogus");

        var result = await harness.RequestAsync<ShowPlanResult>(
            Methods.NQueryShowPlan,
            new ShowPlanParams { TextDocument = new TextDocumentIdentifier { Uri = QueryUri } });

        Assert.NotNull(result.ErrorMessage);
        Assert.Empty(result.Steps);
    }

    private static IEnumerable<ShowPlanNodeInfo> Flatten(ShowPlanNodeInfo node)
    {
        yield return node;

        foreach (var child in node.Children)
        {
            foreach (var descendant in Flatten(child))
                yield return descendant;
        }
    }
}
