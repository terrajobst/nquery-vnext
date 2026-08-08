using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.LanguageServer.Infrastructure;
using NQuery.Northwind;

using LspRange = NQuery.Authoring.LanguageServer.Protocol.Range;

namespace NQuery.LanguageServer;

public sealed class LanguageServerTests
{
    private static readonly Uri DocumentUri = new(@"file:///c:/queries/test.nql");

    private static Task<LanguageServerTestHarness> StartAsync(TimeSpan catalogDelay = default)
    {
        return LanguageServerTestHarness.StartAsync(NorthwindCatalog.Instance, catalogDelay: catalogDelay);
    }

    private static Position At(int line, int character)
    {
        return new Position { Line = line, Character = character };
    }

    private static TextDocumentIdentifier Document()
    {
        return new TextDocumentIdentifier { Uri = DocumentUri };
    }

    [Fact]
    public async Task Initialize_AdvertisesCapabilities()
    {
        await using var harness = await StartAsync();

        var capabilities = harness.InitializeResult.Capabilities;

        Assert.Equal(PositionEncodingKind.Utf16, capabilities.PositionEncoding);
        Assert.Equal(TextDocumentSyncKind.Incremental, capabilities.TextDocumentSync?.Change);
        Assert.True(capabilities.HoverProvider);
        Assert.True(capabilities.DefinitionProvider);
        Assert.True(capabilities.ReferencesProvider);
        Assert.True(capabilities.FoldingRangeProvider);
        Assert.True(capabilities.SelectionRangeProvider);
        Assert.NotNull(capabilities.CompletionProvider);
        Assert.NotNull(capabilities.SemanticTokensProvider);
    }

    [Fact]
    public async Task DidOpen_ValidQuery_ReportsNoDiagnostics()
    {
        await using var harness = await StartAsync();

        var expectation = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT c.CompanyName FROM Customers c");
        var diagnostics = await expectation;

        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task DidOpen_UnknownTable_ReportsError()
    {
        await using var harness = await StartAsync();

        var expectation = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT * FROM Bogus");
        var diagnostics = await expectation;

        // The unknown table also cascades into "must specify table to select from", so this
        // asserts on the specific diagnostic rather than the count.
        var error = Assert.Single(diagnostics, d => d.Code == @"UndeclaredTable");
        Assert.Equal(DiagnosticSeverity.Error, error.Severity);
        Assert.Equal(@"nquery", error.Source);
        Assert.Contains(@"Bogus", error.Message);
        Assert.Equal(14, error.Range.Start.Character);
        Assert.Equal(19, error.Range.End.Character);
    }

    [Fact]
    public async Task DidOpen_SyntaxError_ReportsError()
    {
        await using var harness = await StartAsync();

        var expectation = harness.ExpectDiagnostics(DocumentUri);

        // An unterminated string literal binds fine -- it is still a string column -- so the
        // only thing wrong with this document is lexical. SemanticModel.GetDiagnostics() returns
        // binding diagnostics only, so the syntax tree has to be consulted separately or this
        // produces no squiggle at all.
        await harness.OpenAsync(DocumentUri, @"SELECT 'unterminated");
        var diagnostics = await expectation;

        Assert.NotEmpty(diagnostics);
    }

    [Fact]
    public async Task DidChange_Incremental_UpdatesDiagnostics()
    {
        await using var harness = await StartAsync();

        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT * FROM Bogus");
        Assert.NotEmpty(await opened);

        // Replace "Bogus" with "Customers" -- an incremental, range-based change.
        var changed = harness.ExpectDiagnostics(DocumentUri);
        await harness.ChangeAsync(DocumentUri, 2, new TextDocumentContentChangeEvent
        {
            Range = new LspRange { Start = At(0, 14), End = At(0, 19) },
            Text = @"Customers"
        });

        var diagnostics = await changed;
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task DidChange_AppliesMultipleChangesSequentially()
    {
        await using var harness = await StartAsync();

        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT * FROM Bogus");
        await opened;

        // Two edits in one notification. LSP applies them in order, each against the result of
        // the previous one, so the second range refers to text the first one produced.
        var changed = harness.ExpectDiagnostics(DocumentUri);
        await harness.ChangeAsync(DocumentUri, 2,
            new TextDocumentContentChangeEvent
            {
                Range = new LspRange { Start = At(0, 14), End = At(0, 19) },
                Text = @"Customers"
            },
            new TextDocumentContentChangeEvent
            {
                Range = new LspRange { Start = At(0, 23), End = At(0, 23) },
                Text = @" c"
            });

        var diagnostics = await changed;
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public async Task DidClose_ClearsDiagnostics()
    {
        await using var harness = await StartAsync();

        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT * FROM Bogus");
        Assert.NotEmpty(await opened);

        var closed = harness.ExpectDiagnostics(DocumentUri);
        await harness.CloseAsync(DocumentUri);

        Assert.Empty(await closed);
    }

    [Fact]
    public async Task Completion_AfterFrom_IncludesTables()
    {
        await using var harness = await StartAsync();

        const string text = @"SELECT * FROM ";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var completions = await harness.RequestAsync<CompletionList>(
            Methods.TextDocumentCompletion,
            new CompletionParams { TextDocument = Document(), Position = At(0, text.Length) });

        var labels = completions.Items.Select(i => i.Label).ToArray();
        Assert.Contains(@"Customers", labels);
        Assert.Contains(@"Orders", labels);
        Assert.Contains(@"Order Details", labels);
    }

    [Fact]
    public async Task Completion_ReplacesPartialIdentifier()
    {
        await using var harness = await StartAsync();

        const string text = @"SELECT * FROM Cust";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var completions = await harness.RequestAsync<CompletionList>(
            Methods.TextDocumentCompletion,
            new CompletionParams { TextDocument = Document(), Position = At(0, text.Length) });

        var item = Assert.Single(completions.Items, i => i.Label == @"Customers");
        Assert.NotNull(item.TextEdit);

        // The edit replaces the partial token, not just the caret position.
        Assert.Equal(14, item.TextEdit.Range.Start.Character);
        Assert.Equal(18, item.TextEdit.Range.End.Character);
        Assert.Equal(@"Customers", item.TextEdit.NewText);
    }

    [Fact]
    public async Task Completion_ReplacesPartialBracketedIdentifier()
    {
        await using var harness = await StartAsync();

        // A partially typed bracketed name: the applicable span has to cover the whole token
        // including the opening bracket and the embedded space, which is precisely what a
        // client-side word-boundary guess gets wrong.
        const string text = @"SELECT * FROM [Order Deta";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var completions = await harness.RequestAsync<CompletionList>(
            Methods.TextDocumentCompletion,
            new CompletionParams { TextDocument = Document(), Position = At(0, text.Length) });

        var item = completions.Items.FirstOrDefault(i => i.Label == @"Order Details");
        Assert.True(item is not null, $"Labels: {string.Join(@" | ", completions.Items.Select(i => $"{i.Label}=>{i.TextEdit?.NewText}@{i.TextEdit?.Range.Start.Character}..{i.TextEdit?.Range.End.Character}"))}");
        Assert.NotNull(item.TextEdit);
        Assert.Equal(14, item.TextEdit.Range.Start.Character);
        Assert.Equal(text.Length, item.TextEdit.Range.End.Character);
        Assert.Equal(@"[Order Details]", item.TextEdit.NewText);
    }

    [Fact]
    public async Task Completion_InsideResolvedTableReference_OmitsThatTable()
    {
        await using var harness = await StartAsync();

        // Documents the authoring layer's behavior rather than the server's: a table that is
        // already the resolved reference at the caret is not re-proposed. It is not specific to
        // bracketed names -- plain identifiers behave the same way.
        const string text = @"SELECT * FROM Customers";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var completions = await harness.RequestAsync<CompletionList>(
            Methods.TextDocumentCompletion,
            new CompletionParams { TextDocument = Document(), Position = At(0, 20) });

        var labels = completions.Items.Select(i => i.Label).ToArray();
        Assert.Contains(@"Orders", labels);
        Assert.DoesNotContain(@"Customers", labels);
    }

    [Fact]
    public async Task Hover_OnColumn_ReturnsMarkdown()
    {
        await using var harness = await StartAsync();

        const string text = @"SELECT c.CompanyName FROM Customers c";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var hover = await harness.RequestAsync<Hover?>(
            Methods.TextDocumentHover,
            new HoverParams { TextDocument = Document(), Position = At(0, 12) });

        Assert.NotNull(hover);
        Assert.Equal(MarkupKind.Markdown, hover.Contents.Kind);
        Assert.Contains(@"CompanyName", hover.Contents.Value);
        Assert.Contains(@"```nquery", hover.Contents.Value);
    }

    [Fact]
    public async Task SemanticTokens_ProducesFiveIntegersPerToken()
    {
        await using var harness = await StartAsync();

        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT c.CompanyName FROM Customers c");
        await opened;

        var tokens = await harness.RequestAsync<SemanticTokens>(
            Methods.TextDocumentSemanticTokensFull,
            new SemanticTokensParams { TextDocument = Document() });

        Assert.NotEmpty(tokens.Data);
        Assert.Equal(0, tokens.Data.Length % 5);
    }

    [Fact]
    public async Task SemanticTokens_SplitsMultiLineComments()
    {
        await using var harness = await StartAsync();

        const string text = "/* first\nsecond */\nSELECT 1";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var tokens = await harness.RequestAsync<SemanticTokens>(
            Methods.TextDocumentSemanticTokensFull,
            new SemanticTokensParams { TextDocument = Document() });

        // LSP forbids a token from spanning lines, so every emitted length must fit on its line.
        var lines = text.Replace("\r\n", "\n").Split('\n');
        var line = 0;
        var start = 0;

        for (var i = 0; i < tokens.Data.Length; i += 5)
        {
            var deltaLine = tokens.Data[i];
            var deltaStart = tokens.Data[i + 1];
            var length = tokens.Data[i + 2];

            line += deltaLine;
            start = deltaLine == 0 ? start + deltaStart : deltaStart;

            Assert.True(start + length <= lines[line].Length,
                        $"Token at line {line} col {start} length {length} runs past the end of the line.");
        }
    }

    [Fact]
    public async Task FoldingRange_ReportsMultiLineRegions()
    {
        await using var harness = await StartAsync();

        const string text = "SELECT c.CompanyName\nFROM Customers c\nWHERE c.Country = 'Germany'";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var ranges = await harness.RequestAsync<FoldingRange[]>(
            Methods.TextDocumentFoldingRange,
            new FoldingRangeParams { TextDocument = Document() });

        Assert.NotEmpty(ranges);
        Assert.All(ranges, r => Assert.True(r.EndLine > r.StartLine));
    }

    [Fact]
    public async Task Definition_OnCommonTableExpressionReference_FindsDeclaration()
    {
        await using var harness = await StartAsync();

        const string text = "WITH Recent AS (SELECT * FROM Orders)\nSELECT * FROM Recent";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var locations = await harness.RequestAsync<Location[]?>(
            Methods.TextDocumentDefinition,
            new DefinitionParams { TextDocument = Document(), Position = At(1, 16) });

        Assert.NotNull(locations);
        var location = Assert.Single(locations);
        Assert.Equal(0, location.Range.Start.Line);
        Assert.Equal(5, location.Range.Start.Character);
    }

    [Fact]
    public async Task References_OnAlias_FindsAllUsages()
    {
        await using var harness = await StartAsync();

        const string text = @"SELECT c.CompanyName, c.Country FROM Customers c";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var locations = await harness.RequestAsync<Location[]?>(
            Methods.TextDocumentReferences,
            new ReferenceParams
            {
                TextDocument = Document(),
                Position = At(0, 7),
                Context = new ReferenceContext { IncludeDeclaration = true }
            });

        Assert.NotNull(locations);

        // Two references (c.CompanyName, c.Country) plus the alias declaration.
        Assert.Equal(3, locations.Length);
    }

    [Fact]
    public async Task DocumentHighlight_OnKeyword_HighlightsClause()
    {
        await using var harness = await StartAsync();

        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT * FROM Customers");
        await opened;

        var highlights = await harness.RequestAsync<DocumentHighlight[]?>(
            Methods.TextDocumentDocumentHighlight,
            new DocumentHighlightParams { TextDocument = Document(), Position = At(0, 0) });

        Assert.NotNull(highlights);
        Assert.NotEmpty(highlights);
    }

    [Fact]
    public async Task SignatureHelp_InFunctionCall_ReportsParameters()
    {
        await using var harness = await StartAsync();

        const string text = @"SELECT SUBSTRING('abc', 1, 2)";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var help = await harness.RequestAsync<SignatureHelp?>(
            Methods.TextDocumentSignatureHelp,
            new SignatureHelpParams { TextDocument = Document(), Position = At(0, 18) });

        Assert.NotNull(help);
        Assert.NotEmpty(help.Signatures);

        // Parameter labels are offset pairs into the signature's own text.
        var signature = help.Signatures[0];
        Assert.NotNull(signature.Parameters);
        Assert.All(signature.Parameters, p =>
        {
            Assert.Equal(2, p.Label.Length);
            Assert.True(p.Label[0] >= 0);
            Assert.True(p.Label[1] <= signature.Label.Length);
        });
    }

    [Fact]
    public async Task SelectionRange_WidensThroughParents()
    {
        await using var harness = await StartAsync();

        const string text = @"SELECT c.CompanyName FROM Customers c";
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;

        var ranges = await harness.RequestAsync<SelectionRange[]>(
            Methods.TextDocumentSelectionRange,
            new SelectionRangeParams { TextDocument = Document(), Positions = [At(0, 12)] });

        var range = Assert.Single(ranges);

        // Each parent must strictly contain its child.
        var current = range;
        var depth = 0;

        while (current.Parent is not null)
        {
            var parent = current.Parent;
            Assert.True(parent.Range.Start.Character <= current.Range.Start.Character);
            Assert.True(parent.Range.End.Character >= current.Range.End.Character);
            current = parent;
            depth++;
        }

        Assert.True(depth > 0, @"Expected the selection to widen at least once.");
    }

    [Fact]
    public async Task Request_ArrivingBeforeCatalogResolves_StillUsesCatalog()
    {
        // The catalog takes noticeably longer than the request that follows it; the server must
        // await resolution rather than answering against Catalog.Empty.
        await using var harness = await StartAsync(catalogDelay: TimeSpan.FromMilliseconds(400));

        const string text = @"SELECT * FROM ";
        await harness.OpenAsync(DocumentUri, text);

        var completions = await harness.RequestAsync<CompletionList>(
            Methods.TextDocumentCompletion,
            new CompletionParams { TextDocument = Document(), Position = At(0, text.Length) });

        Assert.Contains(@"Customers", completions.Items.Select(i => i.Label));
    }
}
