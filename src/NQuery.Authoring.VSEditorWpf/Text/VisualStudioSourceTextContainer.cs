using System;
using System.Collections.Generic;
using System.Linq;

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

        public override IEnumerable<TextChange> GetChanges(SourceText newText, SourceText oldText)
        {
            var newTextVs = newText as VisualStudioSourceText;
            var oldTextVs = oldText as VisualStudioSourceText;

            if (newTextVs != null &&
                oldTextVs != null &&
                newTextVs.Snapshot.TextBuffer == oldTextVs.Snapshot.TextBuffer &&
                newTextVs.Snapshot.Version.VersionNumber > oldTextVs.Snapshot.Version.VersionNumber)
            {
                return GetChanges(oldTextVs, newTextVs);
            }

            return base.GetChanges(newText, oldText);
        }

        private static IEnumerable<TextChange> GetChanges(VisualStudioSourceText oldTextVs, VisualStudioSourceText newTextVs)
        {
            var oldVersion = oldTextVs.Snapshot.Version;
            var newVersion = newTextVs.Snapshot.Version;

            var current = oldVersion;

            while (current != newVersion)
            {
                foreach (var change in current.Changes.Reverse())
                {
                    var changeSpan = new TextSpan(change.OldSpan.Start, change.OldSpan.Length);
                    var changeText = change.NewText;
                    yield return new TextChange(changeSpan, changeText);
                }
                current = current.Next;
            }
        }

        public override event EventHandler<EventArgs> CurrentChanged;
    }
}