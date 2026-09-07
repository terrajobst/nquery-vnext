using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.Tests;

public class DocumentTests
{
    [Fact]
    public void Document_Create_AllowsANullFilePath()
    {
        Assert.Null(CreateDocument().FilePath);
    }

    [Fact]
    public void Document_Create_KeepsTheFilePath()
    {
        Assert.Equal(FilePath, CreateDocument(FilePath).FilePath);
    }

    [Fact]
    public void Document_WithFilePath_ReplacesTheFilePath()
    {
        var document = CreateDocument().WithFilePath(FilePath);

        Assert.Equal(FilePath, document.FilePath);
        Assert.Null(document.WithFilePath(null).FilePath);
    }

    [Fact]
    public void Document_WithFilePath_ReturnsTheSameDocumentWhenUnchanged()
    {
        var document = CreateDocument(FilePath);

        Assert.Same(document, document.WithFilePath(FilePath));
    }

    [Fact]
    public void Document_WithText_KeepsTheFilePath()
    {
        var document = CreateDocument(FilePath);

        Assert.Equal(FilePath, document.WithText(SourceText.From(@"SELECT 2")).FilePath);
    }

    [Fact]
    public void Document_WithKind_KeepsTheFilePath()
    {
        var document = CreateDocument(FilePath);

        Assert.Equal(FilePath, document.WithKind(DocumentKind.Expression).FilePath);
    }

    [Fact]
    public void Document_WithCatalog_KeepsTheFilePath()
    {
        var document = CreateDocument(FilePath);

        Assert.Equal(FilePath, document.WithCatalog(Catalog.Default).FilePath);
    }

    [Fact]
    public void Document_WithServices_KeepsTheFilePath()
    {
        var document = CreateDocument(FilePath);

        Assert.Equal(FilePath, document.WithServices(AuthoringServices.Create()).FilePath);
    }

    private const string FilePath = @"/repo/src/query.nql";

    private static Document CreateDocument(string? filePath = null)
    {
        return Document.Create(DocumentKind.Query, SourceText.From(@"SELECT 1"), filePath, Catalog.Empty, AuthoringServices.Create());
    }
}
