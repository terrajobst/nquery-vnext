using System;

using ActiproSoftware.Text;

using NQuery.Authoring.Document;

namespace NQuery.Authoring.ActiproWpf.Text
{
    internal sealed class SnapshotTextBufferPovider : TextBufferProvider
    {
        private readonly ITextDocument _textBuffer;

        public SnapshotTextBufferPovider(ITextDocument textDocument)
        {
            _textBuffer = textDocument;
            _textBuffer.TextChanged += TextBufferOnTextChanged;
            UpdateCurrent();
        }

        private void UpdateCurrent()
        {
            Current = new SnapshotTextBuffer(_textBuffer.CurrentSnapshot);
        }

        private void TextBufferOnTextChanged(object sender, TextSnapshotChangedEventArgs e)
        {
            UpdateCurrent();
        }
    }
}