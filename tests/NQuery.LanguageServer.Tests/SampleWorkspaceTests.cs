using NQuery.Authoring.LanguageServer.Protocol;
using NQuery.LanguageServer.Infrastructure;
using NQuery.Northwind;

namespace NQuery.LanguageServer;

// The sample workspace is what a developer opens to try the extension, so a broken sample is a
// broken first impression. These run the real files through the real server.
public sealed class SampleWorkspaceTests
{
    public static TheoryData<string> SampleFiles()
    {
        var data = new TheoryData<string>();

        var extensions = new[] { @".nql", @".nqe" };

        foreach (var file in Directory.EnumerateFiles(GetSamplesDirectory(), @"*", SearchOption.AllDirectories))
        {
            if (extensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                data.Add(Path.GetFileName(file));
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(SampleFiles))]
    public async Task SampleQuery_HasNoErrors(string fileName)
    {
        var path = Directory.EnumerateFiles(GetSamplesDirectory(), fileName, SearchOption.AllDirectories).Single();
        var text = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);
        var uri = new Uri(path);

        await using var harness = await LanguageServerTestHarness.StartAsync(NorthwindCatalog.Instance);

        var expectation = harness.ExpectDiagnostics(uri);
        await harness.OpenAsync(uri, text);
        var diagnostics = await expectation.WaitAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.True(errors.Length == 0,
                    $"{fileName}: {string.Join(@"; ", errors.Select(e => $"({e.Range.Start.Line + 1},{e.Range.Start.Character + 1}) {e.Message}"))}");
    }

    [Fact]
    public void SampleProjectFile_Exists()
    {
        var projectFile = Path.Combine(GetSamplesDirectory(), @"northwind.nqproj");
        Assert.True(File.Exists(projectFile), $"Expected a project file at {projectFile}.");
    }

    // Walks up from the test binary to the repository root; the samples live outside the
    // artifacts tree, so there is nothing to copy to the output directory.
    private static string GetSamplesDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, @"samples", @"northwind");
            if (Directory.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(@"Could not locate the samples\northwind directory.");
    }
}
