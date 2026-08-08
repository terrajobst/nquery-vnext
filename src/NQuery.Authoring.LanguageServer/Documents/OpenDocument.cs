using NQuery.Authoring.LanguageServer.Text;

namespace NQuery.Authoring.LanguageServer.Documents;

internal sealed class OpenDocument
{
    public OpenDocument(Uri uri, string languageId, int version, string text, DocumentKind kind, Catalog catalog)
    {
        ThrowIfNull(uri);
        ThrowIfNull(languageId);
        ThrowIfNull(text);
        ThrowIfNull(catalog);

        Uri = uri;
        LanguageId = languageId;
        Version = version;
        Container = new LspSourceTextContainer(text);
        Workspace = new Workspace(Container)
        {
            DocumentKind = kind,
            Catalog = catalog
        };
    }

    public Uri Uri { get; }

    public string LanguageId { get; }

    public int Version { get; set; }

    public LspSourceTextContainer Container { get; }

    public Workspace Workspace { get; }
}
