using NQuery.Authoring.LanguageServer;
using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.LanguageServer.Infrastructure;
using NQuery.Northwind;

using LspCodeAction = NQuery.Authoring.LanguageServer.Protocol.CodeAction;
using LspRange = NQuery.Authoring.LanguageServer.Protocol.Range;

namespace NQuery.LanguageServer;

public sealed class CodeActionTests
{
    private static readonly Uri DocumentUri = new(@"file:///c:/queries/actions.nql");

    private static async Task<LanguageServerTestHarness> OpenAsync(string text, Action<NQueryLanguageServerOptions>? configure = null)
    {
        var harness = await LanguageServerTestHarness.StartAsync(NorthwindCatalog.Instance, configure: configure);
        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, text);
        await opened;
        return harness;
    }

    private static Task<LspCodeAction[]> RequestAsync(LanguageServerTestHarness harness, int line, int character, string? only = null)
    {
        var position = new Position { Line = line, Character = character };

        return harness.RequestAsync<LspCodeAction[]>(
            Methods.TextDocumentCodeAction,
            new CodeActionParams
            {
                TextDocument = new TextDocumentIdentifier { Uri = DocumentUri },
                Range = new LspRange { Start = position, End = position },
                Context = only is null ? null : new CodeActionContext { Only = [only] }
            });
    }

    /// Applies an action's edits the way a client would: against the original text, and in
    /// descending order so earlier offsets stay valid.
    private static string Apply(string text, LspCodeAction action)
    {
        var edits = action.Edit!.Changes[DocumentUri.OriginalString];
        var lines = text.Replace("\r\n", "\n").Split('\n');

        int Offset(Position p) => lines.Take(p.Line).Sum(l => l.Length + 1) + p.Character;

        var ordered = edits.OrderByDescending(e => Offset(e.Range.Start)).ToArray();
        var result = text.Replace("\r\n", "\n");

        foreach (var edit in ordered)
        {
            var start = Offset(edit.Range.Start);
            var end = Offset(edit.Range.End);
            result = result[..start] + edit.NewText + result[end..];
        }

        return result;
    }

    [Fact]
    public async Task CodeAction_OffersRefactoringAtPosition()
    {
        // 'c' is an alias without AS, which AddAsAliasCodeRefactoringProvider offers to add.
        const string text = @"SELECT * FROM Customers c";
        await using var harness = await OpenAsync(text);

        var actions = await RequestAsync(harness, 0, text.Length - 1);

        Assert.NotEmpty(actions);
        Assert.All(actions, a => Assert.NotNull(a.Edit));
        Assert.All(actions, a => Assert.NotEmpty(a.Title));
    }

    [Fact]
    public async Task CodeAction_EditIsPreciseRatherThanWholeDocument()
    {
        const string text = @"SELECT * FROM Customers c";
        await using var harness = await OpenAsync(text);

        var actions = await RequestAsync(harness, 0, text.Length - 1);
        var action = Assert.Single(actions, a => a.Title.Contains(@"AS", StringComparison.OrdinalIgnoreCase));

        var edit = Assert.Single(action.Edit!.Changes[DocumentUri.OriginalString]);

        // The whole point of going through GetChanges rather than diffing whole texts: the edit
        // touches the alias, not the document, so the cursor and folding survive.
        Assert.Equal(@"AS ", edit.NewText);
        Assert.Equal(24, edit.Range.Start.Character);
        Assert.Equal(24, edit.Range.End.Character);
    }

    [Fact]
    public async Task CodeAction_AppliedEditProducesTheExpectedText()
    {
        const string text = @"SELECT * FROM Customers c";
        await using var harness = await OpenAsync(text);

        var actions = await RequestAsync(harness, 0, text.Length - 1);
        var action = Assert.Single(actions, a => a.Title.Contains(@"AS", StringComparison.OrdinalIgnoreCase));

        Assert.Equal(@"SELECT * FROM Customers AS c", Apply(text, action));
    }

    [Fact]
    public async Task CodeAction_ExpandsWildcard()
    {
        // A multi-edit case, and one where getting the ranges wrong would be obvious.
        const string text = @"SELECT * FROM Shippers s";
        await using var harness = await OpenAsync(text);

        var actions = await RequestAsync(harness, 0, 7);
        var action = actions.FirstOrDefault(a => a.Title.Contains(@"*", StringComparison.Ordinal)
                                              || a.Title.Contains(@"xpand", StringComparison.Ordinal));

        Assert.True(action is not null,
                    $"Titles: {string.Join(@" | ", actions.Select(a => a.Title))}");

        var applied = Apply(text, action);
        Assert.DoesNotContain(@"*", applied);
        Assert.Contains(@"ShipperID", applied);
    }

    [Fact]
    public async Task CodeAction_OffersFixForAnIssue()
    {
        // ComparisonWithNullCodeIssueProvider flags '= NULL' and offers to rewrite it as IS NULL.
        const string text = @"SELECT * FROM Customers c WHERE c.Region = NULL";
        await using var harness = await OpenAsync(text);

        var actions = await RequestAsync(harness, 0, text.IndexOf(@"= NULL", StringComparison.Ordinal));

        Assert.NotEmpty(actions);
        Assert.Contains(actions, a => a.Kind == CodeActionKind.QuickFix);

        var fix = actions.First(a => a.Kind == CodeActionKind.QuickFix);
        Assert.Contains(@"IS NULL", Apply(text, fix), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CodeAction_HonoursTheRequestedKind()
    {
        const string text = @"SELECT * FROM Customers c WHERE c.Region = NULL";
        await using var harness = await OpenAsync(text);

        var offset = text.IndexOf(@"= NULL", StringComparison.Ordinal);

        var refactorings = await RequestAsync(harness, 0, offset, CodeActionKind.Refactor);
        Assert.All(refactorings, a => Assert.Equal(CodeActionKind.Refactor, a.Kind));

        var fixes = await RequestAsync(harness, 0, offset, CodeActionKind.QuickFix);
        Assert.All(fixes, a => Assert.Equal(CodeActionKind.QuickFix, a.Kind));
    }

    [Fact]
    public async Task CodeAction_ReturnsNothingWhereNoActionApplies()
    {
        await using var harness = await OpenAsync("SELECT 1\n\n");

        var actions = await RequestAsync(harness, 1, 0);

        Assert.Empty(actions);
    }

    [Fact]
    public async Task CodeAction_ReturnsNothingWhenTheCatalogIsUnavailable()
    {
        // Without a catalog nothing binds, so any action offered would be reasoning about a
        // document it cannot resolve.
        await using var harness = await LanguageServerTestHarness.StartAsync(
            catalogProvider: new ThrowingCatalogProvider(@"no connection"));

        var opened = harness.ExpectDiagnostics(DocumentUri);
        await harness.OpenAsync(DocumentUri, @"SELECT * FROM Customers c");
        await opened;

        Assert.Empty(await RequestAsync(harness, 0, 24));
    }

    [Fact]
    public async Task CodeAction_DoesNotOfferTheSameActionTwice()
    {
        const string text = @"SELECT * FROM Customers c WHERE c.Region = NULL";
        await using var harness = await OpenAsync(text);

        var actions = await RequestAsync(harness, 0, text.IndexOf(@"= NULL", StringComparison.Ordinal));

        var duplicates = actions.GroupBy(a => a.Title).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
        Assert.True(duplicates.Length == 0, $"Duplicated: {string.Join(@", ", duplicates)}");
    }

    [Fact]
    public async Task Initialize_AdvertisesCodeActionSupport()
    {
        await using var harness = await LanguageServerTestHarness.StartAsync(NorthwindCatalog.Instance);

        var provider = harness.InitializeResult.Capabilities.CodeActionProvider;

        Assert.NotNull(provider);
        Assert.Contains(CodeActionKind.QuickFix, provider.CodeActionKinds!);
        Assert.Contains(CodeActionKind.Refactor, provider.CodeActionKinds!);
    }
}
