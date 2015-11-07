using System;

using Microsoft.VisualStudio.Text;

using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Text
{
    internal sealed class VisualStudioSourceTextContainer : SourceTextContainer
    {
        public VisualStudioSourceTextContainer(ITextBuffer textBuffer)
        {
            TextBuffer = textBuffer;
            TextBuffer.ChangedHighPriority += TextBufferOnPostChanged;
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
            get { return TextBuffer.CurrentSnapshot.ToSourceText(); }
        }

        public ITextBuffer TextBuffer { get; }

        public override event EventHandler<EventArgs> CurrentChanged;
    }
}