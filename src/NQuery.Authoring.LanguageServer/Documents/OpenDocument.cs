using NQuery.Authoring.LanguageServer.Text;

namespace NQuery.Authoring.LanguageServer.Documents;

// The mutable state the server keeps around one open file: the text container the client writes
// into, and the last Document snapshot taken from it.
//
// Not thread-safe by design -- every member is called under DocumentStore's lock, which is also
// what serializes the text changes this reads.
internal sealed class OpenDocument
{
    private Document _document;

    public OpenDocument(Uri uri, string languageId, int version, string text, DocumentKind kind, Catalog catalog, AuthoringServices services)
    {
        ThrowIfNull(uri);
        ThrowIfNull(languageId);
        ThrowIfNull(text);
        ThrowIfNull(catalog);
        ThrowIfNull(services);

        Uri = uri;
        LanguageId = languageId;
        Version = version;
        Container = new LspSourceTextContainer(text);

        // Only a file: URI names something on disk. An untitled buffer or a document the client
        // addresses some other way has no path, and a service resolving settings from the file
        // system needs to be told that rather than left to guess.
        var filePath = uri.IsFile ? uri.LocalPath : null;
        _document = Document.Create(kind, Container.Current, filePath, catalog, services);
    }

    public Uri Uri { get; }

    public string LanguageId { get; }

    public int Version { get; set; }

    public LspSourceTextContainer Container { get; }

    // A method rather than a property because it isn't a read: changes land on the container, and
    // the document is only brought forward when someone actually needs a snapshot.
    public Document GetDocument()
    {
        if (_document.Text != Container.Current)
            _document = _document.WithText(Container.Current);

        return _document;
    }

    public void SetCatalog(Catalog catalog)
    {
        ThrowIfNull(catalog);

        _document = GetDocument().WithCatalog(catalog);
    }
}
