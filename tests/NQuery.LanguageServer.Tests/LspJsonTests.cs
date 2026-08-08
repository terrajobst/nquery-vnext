using System.Text.Json;

using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.Authoring.LanguageServer.Server;
using NQuery.LanguageServer.Infrastructure;
using NQuery.Northwind;

namespace NQuery.LanguageServer;

// The rest of the suite hardcodes URI strings, which are already well-formed. These cover the
// other way a URI reaches the wire: built from a local path, as a client (or a test) does.
public sealed class LspJsonTests
{
    [Fact]
    public void DocumentUri_FromLocalPath_IsWrittenAsAbsoluteUri()
    {
        var uri = new Uri(Path.Combine(Path.GetTempPath(), @"query.nql"));

        var json = JsonSerializer.Serialize(uri, LspJson.CreateOptions());

        // The default converter would write Uri.OriginalString here, which on a Unix path is the
        // bare "/tmp/query.nql" -- not a URI, and no longer absolute once parsed back.
        Assert.StartsWith(@"""file://", json);
    }

    [Fact]
    public void DocumentUri_FromLocalPath_SurvivesRoundTrip()
    {
        var options = LspJson.CreateOptions();
        var uri = new Uri(Path.Combine(Path.GetTempPath(), @"query.nql"));

        var roundTripped = JsonSerializer.Deserialize<Uri>(JsonSerializer.Serialize(uri, options), options);

        // Equality is what matters: the server echoes the URI back and clients match publishes
        // against the one they sent.
        Assert.True(roundTripped!.IsAbsoluteUri);
        Assert.Equal(uri, roundTripped);
    }

    [Fact]
    public void DocumentUri_FromBarePath_IsStillUnderstood()
    {
        var options = LspJson.CreateOptions();

        var uri = JsonSerializer.Deserialize<Uri>(@"""/queries/query.nql""", options);

        Assert.Equal(@"/queries/query.nql", uri!.OriginalString);
    }

    // The serialization tests above pin the wire format; this one proves it end to end, so a
    // regression shows up as a failing publish rather than only as changed JSON.
    [Fact]
    public async Task Diagnostics_ArePublished_ForUriBuiltFromLocalPath()
    {
        var uri = new Uri(Path.Combine(Path.GetTempPath(), @"local-path.nql"));

        await using var harness = await LanguageServerTestHarness.StartAsync(NorthwindCatalog.Instance);

        var expectation = harness.ExpectDiagnostics(uri);
        await harness.OpenAsync(uri, @"SELECT * FROM Bogus");
        var diagnostics = await expectation.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        Assert.Contains(diagnostics, d => d.Code == @"UndeclaredTable");
    }
}
