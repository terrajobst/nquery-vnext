using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring;

// A mutable view over a text container that keeps the current Document snapshot up to date.
//
// One of these per open document. The service composition is deliberately not built here: it is
// created once per host and handed in, because a workspace is per-file while a composition is per
// process.
public sealed class Workspace
{
    private Document _currentDocument;

    private Workspace(SourceTextContainer textContainer, DocumentKind kind, AuthoringServices services)
    {
        TextContainer = textContainer;
        TextContainer.CurrentChanged += TextContainerOnCurrentChanged;
        _currentDocument = Document.Create(kind, textContainer.Current, Catalog.Empty, services);
    }

    public static Workspace Create(SourceTextContainer textContainer, DocumentKind kind, AuthoringServices services)
    {
        ThrowIfNull(textContainer);
        ThrowIfNull(services);

        return new Workspace(textContainer, kind, services);
    }

    // Creation-time only: a document's kind comes from what was opened and never changes over the
    // life of the workspace, unlike the catalog, which a host watching a live schema replaces.
    public DocumentKind DocumentKind
    {
        get { return CurrentDocument.Kind; }
    }

    public Catalog Catalog
    {
        get { return CurrentDocument.Catalog; }
        set { CurrentDocument = CurrentDocument.WithCatalog(value); }
    }

    public AuthoringServices Services
    {
        get { return CurrentDocument.Services; }
        set { CurrentDocument = CurrentDocument.WithServices(value); }
    }

    public SourceTextContainer TextContainer { get; }

    public Document CurrentDocument
    {
        get
        {
            // Ensure the document is up-to-date
            if (_currentDocument.Text != TextContainer.Current)
                _currentDocument = _currentDocument.WithText(TextContainer.Current);

            return _currentDocument;
        }
        private set
        {
            if (_currentDocument != value)
            {
                _currentDocument = value;
                OnCurrentDocumentChanged();
            }
        }
    }

    private void TextContainerOnCurrentChanged(object? sender, EventArgs e)
    {
        OnCurrentDocumentChanged();
    }

    private void OnCurrentDocumentChanged()
    {
        var handler = CurrentDocumentChanged;
        handler?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<EventArgs>? CurrentDocumentChanged;
}
