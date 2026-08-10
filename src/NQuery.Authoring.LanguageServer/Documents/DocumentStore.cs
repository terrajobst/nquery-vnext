using System.Collections.Immutable;

using NQuery.Authoring.LanguageServer.Protocol;

namespace NQuery.Authoring.LanguageServer.Documents;

// Owns every open document for this server (i.e. for one .nqproj project).
//
// Requests are dispatched concurrently by StreamJsonRpc, so all access is serialized by a plain
// lock. The lock is only ever held long enough to swap text or take a Document snapshot -- never
// across compilation. Document is immutable and computes its syntax tree / semantic model behind
// Interlocked, so once a handler has a snapshot it can do the expensive work without the lock.
internal sealed class DocumentStore
{
    private readonly Dictionary<Uri, OpenDocument> _documents = new();
    private readonly object _gate = new();

    // Built once per server and shared by every open document: a workspace is per file, while the
    // service composition is per process.
    private readonly AuthoringServices _services;

    private Catalog _catalog = Catalog.Empty;

    public DocumentStore(AuthoringServices services)
    {
        ThrowIfNull(services);

        _services = services;
    }

    public Catalog Catalog
    {
        get
        {
            lock (_gate)
                return _catalog;
        }
    }

    public void Open(Uri uri, string languageId, int version, string text)
    {
        ThrowIfNull(uri);
        ThrowIfNull(languageId);
        ThrowIfNull(text);

        lock (_gate)
        {
            var kind = DocumentKindMapping.FromUri(uri);
            _documents[uri] = new OpenDocument(uri, languageId, version, text, kind, _catalog, _services);
        }
    }

    public bool Change(Uri uri, int version, IReadOnlyList<TextDocumentContentChangeEvent> changes)
    {
        ThrowIfNull(uri);
        ThrowIfNull(changes);

        lock (_gate)
        {
            if (!_documents.TryGetValue(uri, out var document))
                return false;

            document.Container.ApplyChanges(changes);
            document.Version = version;
            return true;
        }
    }

    public bool Close(Uri uri)
    {
        ThrowIfNull(uri);

        lock (_gate)
            return _documents.Remove(uri);
    }

    // Takes an immutable snapshot under the lock; the caller compiles outside of it.
    public bool TryGetSnapshot(Uri uri, out DocumentSnapshot snapshot)
    {
        ThrowIfNull(uri);

        lock (_gate)
        {
            if (!_documents.TryGetValue(uri, out var document))
            {
                snapshot = default;
                return false;
            }

            snapshot = new DocumentSnapshot(uri, document.Version, document.Workspace.CurrentDocument);
            return true;
        }
    }

    public ImmutableArray<DocumentSnapshot> GetSnapshots()
    {
        lock (_gate)
        {
            var builder = ImmutableArray.CreateBuilder<DocumentSnapshot>(_documents.Count);

            foreach (var document in _documents.Values)
                builder.Add(new DocumentSnapshot(document.Uri, document.Version, document.Workspace.CurrentDocument));

            return builder.MoveToImmutable();
        }
    }

    // Applied to every open document; returns the affected documents so the caller can
    // re-publish diagnostics for them.
    public ImmutableArray<DocumentSnapshot> SetCatalog(Catalog catalog)
    {
        ThrowIfNull(catalog);

        lock (_gate)
        {
            _catalog = catalog;

            var builder = ImmutableArray.CreateBuilder<DocumentSnapshot>(_documents.Count);

            foreach (var document in _documents.Values)
            {
                document.Workspace.Catalog = catalog;
                builder.Add(new DocumentSnapshot(document.Uri, document.Version, document.Workspace.CurrentDocument));
            }

            return builder.MoveToImmutable();
        }
    }
}

internal readonly record struct DocumentSnapshot(Uri Uri, int Version, Document Document);
