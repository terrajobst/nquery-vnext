using System;

using Microsoft.VisualStudio.Text;

using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Text
{
    internal sealed class VisualStudioSourceTextContainer : SourceTextContainer
    {
        private readonly ITextBuffer _textBuffer;

        public VisualStudioSourceTextContainer(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
            _textBuffer.ChangedHighPriority += TextBufferOnPostChanged;
        }

        private void TextBufferOnPostChanged(object sender, EventArgs e)
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
            get { return _textBuffer.CurrentSnapshot.ToSourceText(); }
        }

        public ITextBuffer TextBuffer
        {
            get { return _textBuffer; }
        }

        public override event EventHandler<EventArgs> CurrentChanged;
    }
}