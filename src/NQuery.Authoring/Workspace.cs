using System;

using NQuery.Text;

namespace NQuery.Authoring
{
    public sealed class Workspace
    {
        private readonly SourceTextContainer _textContainer;
        private Document _currentDocument;

        public Workspace(SourceTextContainer textContainer)
        {
            _textContainer = textContainer;
            _textContainer.CurrentChanged += TextContainerOnCurrentChanged;
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

        public SourceTextContainer TextContainer
        {
            get { return _textContainer; }
        }

        public Document CurrentDocument
        {
            get
            {
                // Ensure the document is up-to-date
                if (_currentDocument.Text != _textContainer.Current)
                    _currentDocument = _currentDocument.WithText(_textContainer.Current);

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