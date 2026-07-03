using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring;

public sealed class Workspace
{
    private Document _currentDocument;

    public Workspace(SourceTextContainer textContainer)
    {
        ThrowIfNull(textContainer);

        TextContainer = textContainer;
        TextContainer.CurrentChanged += TextContainerOnCurrentChanged;
        _currentDocument = new Document(DocumentKind.Query, Catalog.Empty, textContainer.Current);
    }

    public DocumentKind DocumentKind
    {
        get { return CurrentDocument.Kind; }
        set { CurrentDocument = CurrentDocument.WithKind(value); }
    }

    public Catalog Catalog
    {
        get { return CurrentDocument.Catalog; }
        set { CurrentDocument = CurrentDocument.WithCatalog(value); }
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
