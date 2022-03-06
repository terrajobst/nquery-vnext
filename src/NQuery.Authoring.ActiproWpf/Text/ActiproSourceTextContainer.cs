using ActiproSoftware.Text;

using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Text
{
    internal sealed class ActiproSourceTextContainer : SourceTextContainer
    {
        public ActiproSourceTextContainer(ITextDocument textDocument)
        {
            TextDocument = textDocument;
            TextDocument.TextChanged += TextDocumentOnTextChanged;
        }

        private void TextDocumentOnTextChanged(object sender, TextSnapshotChangedEventArgs e)
        {
            OnCurrentChanged();
        }

        private void OnCurrentChanged()
        {
            var handler = CurrentChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public override SourceText Current
        {
            get { return TextDocument.CurrentSnapshot.ToSourceText(); }
        }

        public ITextDocument TextDocument { get; }

        public override event EventHandler<EventArgs> CurrentChanged;
    }
}