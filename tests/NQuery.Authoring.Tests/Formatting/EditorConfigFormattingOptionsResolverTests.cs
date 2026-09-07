using NQuery.Authoring.Configuration;
using NQuery.Authoring.Formatting;
using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests.Formatting;

// The resolver is the only part of this that touches the file system, so these tests write real
// configs into a real directory. What the keys mean is FormattingOptionsEditorConfigTests' job.
public class EditorConfigFormattingOptionsResolverTests
{
    [Fact]
    public void EditorConfigResolver_ReadsTheConfigNextToTheDocument()
    {
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            root = true

            [*.nql]
            indent_size = 2
            nquery_keyword_case = lower
            """);

        var document = CreateDocument(directory.GetPath(@"query.nql"));

        var options = Resolve(document);

        Assert.Equal(2, options.IndentSize);
        Assert.Equal(Casing.Lower, options.Keywords);
    }

    [Fact]
    public void EditorConfigResolver_WalksUpFromTheDocument()
    {
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            root = true

            [*.nql]
            indent_size = 2
            """);

        var document = CreateDocument(directory.GetPath(@"src/queries/query.nql"));

        var options = Resolve(document);

        Assert.Equal(2, options.IndentSize);
    }

    [Fact]
    public void EditorConfigResolver_OverridesOnlyWhatTheConfigMentions()
    {
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            root = true

            [*.nql]
            indent_size = 2
            """);

        var document = CreateDocument(directory.GetPath(@"query.nql"));
        var baseline = FormattingOptions.Stacked with { Keywords = Casing.Preserve };

        var options = Resolve(document, baseline);

        Assert.Equal(2, options.IndentSize);
        Assert.Equal(baseline with { IndentSize = 2 }, options);
    }

    [Fact]
    public void EditorConfigResolver_KeepsTheBaselineWithoutAConfig()
    {
        using var directory = new TempDirectory();

        var document = CreateDocument(directory.GetPath(@"query.nql"));
        var baseline = FormattingOptions.Stacked;

        Assert.Equal(baseline, Resolve(document, baseline));
    }

    [Fact]
    public void EditorConfigResolver_KeepsTheBaselineForADocumentWithoutAPath()
    {
        // A buffer that was never saved is nowhere, so nobody's settings apply to it -- least of
        // all whatever happens to sit above the process's working directory.
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            root = true

            [*]
            indent_size = 2
            """);

        var document = CreateDocument(filePath: null);

        Assert.Equal(FormattingOptions.Default, Resolve(document));
    }

    [Fact]
    public void EditorConfigResolver_IsNotRegisteredByDefault()
    {
        // Reading files is opt-in: the default composition has to stay free of I/O.
        var services = AuthoringServices.Create();

        Assert.Equal(typeof(FormattingOptionsResolver), services.GetService<FormattingOptionsResolver>().GetType());
    }

    [Fact]
    public void FormattingService_FormatsWithTheResolvedOptions()
    {
        using var directory = new TempDirectory();

        directory.CreateFile(EditorConfig.FileName, """
            root = true

            [*.nql]
            nquery_keyword_case = lower
            end_of_line = lf
            """);

        var document = CreateDocument(directory.GetPath(@"query.nql"), @"SELECT 1 FROM Employees");
        var service = document.Services.GetService<FormattingService>();

        var formatted = service.Format(document);

        Assert.Equal("select  1\nfrom    Employees\n", formatted.Text.GetText());
    }

    private static readonly AuthoringServices Services = AuthoringServices.Create(builder =>
    {
        builder.AddDefaultServices();
        builder.RemoveServices<FormattingOptionsResolver>();
        builder.AddService<FormattingOptionsResolver, EditorConfigFormattingOptionsResolver>();
    });

    private static Document CreateDocument(string? filePath, string query = @"SELECT 1")
    {
        return Document.Create(DocumentKind.Query, SourceText.From(query), filePath, Catalog.Default, Services);
    }

    private static FormattingOptions Resolve(Document document, FormattingOptions? options = null)
    {
        var service = document.Services.GetService<FormattingService>();

        return service.GetOptions(document, options ?? FormattingOptions.Default);
    }
}
