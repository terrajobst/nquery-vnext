using System;

using ActiproSoftware.Text;

using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Text
{
    internal sealed class ActiproSourceTextContainer : SourceTextContainer
    {
        private readonly ITextDocument _textDocument;

        public ActiproSourceTextContainer(ITextDocument textDocument)
        {
            _textDocument = textDocument;
            _textDocument.TextChanged += TextDocumentOnTextChanged;
        }

        private void TextDocumentOnTextChanged(object sender, TextSnapshotChangedEventArgs e)
        {
            OnCurrentChanged();
        }

        private void OnCurrentChanged()
        {
            var handler = CurrentChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public override SourceText Current
        {
            get { return _textDocument.CurrentSnapshot.ToSourceText(); }
        }

        public ITextDocument TextDocument
        {
            get { return _textDocument; }
        }

        public override event EventHandler<EventArgs> CurrentChanged;
    }
}