using System;
using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.Text;

using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Text
{
    public static class VisualStudioTextExtensions
    {
        private static readonly ConditionalWeakTable<ITextBuffer, SourceTextContainer> ProviderMap = new ConditionalWeakTable<ITextBuffer, SourceTextContainer>();
        private static readonly ConditionalWeakTable<ITextSnapshot, SourceText> SnapshotMap = new ConditionalWeakTable<ITextSnapshot, SourceText>();

        public static SourceTextContainer ToSourceTextContainer(this ITextBuffer textBuffer)
        {
            if (textBuffer == null)
                throw new ArgumentNullException(nameof(textBuffer));

            return ProviderMap.GetValue(textBuffer, tb => new VisualStudioSourceTextContainer(tb));
        }

        public static SourceText ToSourceText(this ITextSnapshot textSnapshot)
        {
            if (textSnapshot == null)
                throw new ArgumentNullException(nameof(textSnapshot));

            var container = (VisualStudioSourceTextContainer)textSnapshot.TextBuffer.ToSourceTextContainer();
            return SnapshotMap.GetValue(textSnapshot, ts => new VisualStudioSourceText(container, ts));
        }

        public static ITextSnapshot ToTextSnapshot(this SourceText text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var visualStudioSourceText = text as VisualStudioSourceText;
            if (visualStudioSourceText == null)
                throw new ArgumentException("The source text didn't originate from a Visual Studio Editor", nameof(text));

            return visualStudioSourceText.Snapshot;
        }

        public static ITextBuffer ToTextBuffer(this SourceTextContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            var visualStudioSourceTextContainer = container as VisualStudioSourceTextContainer;
            if (visualStudioSourceTextContainer == null)
                throw new ArgumentException("The source text container didn't originate from a Visual Studio Editor", nameof(container));

            return visualStudioSourceTextContainer.TextBuffer;
        }
    }
}