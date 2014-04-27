using System;

using Microsoft.VisualStudio.Text;

using NQuery.Authoring.Document;

namespace NQuery.Authoring.VSEditorWpf.Text
{
    internal sealed class SnapshotTextBufferPovider : TextBufferProvider
    {
        private readonly ITextBuffer _textBuffer;

        public SnapshotTextBufferPovider(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _textBuffer.PostChanged += TextBufferOnPostChanged;
            UpdateCurrent();
        }

        private void UpdateCurrent()
        {
            Current = new SnapshotTextBuffer(_textBuffer.CurrentSnapshot);
        }

        private void TextBufferOnPostChanged(object sender, EventArgs e)
        {
            UpdateCurrent();
        }
    }
}