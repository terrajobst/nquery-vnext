using NQuery.Text;

namespace NQuery.Authoring
{
    public sealed class Workspace
    {
        private Document _currentDocument;

        public Workspace(SourceTextContainer textContainer)
        {
            TextContainer = textContainer;
            TextContainer.CurrentChanged += TextContainerOnCurrentChanged;
            _currentDocument = new Document(DocumentKind.Query, DataContext.Empty, textContainer.Current);
        }

        public DocumentKind DocumentKind
        {
            get { return CurrentDocument.Kind; }
            set { CurrentDocument = CurrentDocument.WithKind(value); }
        }

        public DataContext DataContext
        {
            get { return CurrentDocument.DataContext; }
            set { CurrentDocument = CurrentDocument.WithDataContext(value); }
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

        private void TextContainerOnCurrentChanged(object sender, EventArgs e)
        {
            OnCurrentDocumentChanged();
        }

        private void OnCurrentDocumentChanged()
        {
            var handler = CurrentDocumentChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs> CurrentDocumentChanged;
    }
}