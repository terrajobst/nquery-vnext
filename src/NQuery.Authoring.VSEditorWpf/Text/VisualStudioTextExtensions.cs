using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.Text;

using NQuery.Text;

namespace NQuery.Authoring.VSEditorWpf.Text
{
    public static class VisualStudioTextExtensions
    {
        private static readonly ConditionalWeakTable<ITextBuffer, SourceTextContainer> ProviderMap = new();
        private static readonly ConditionalWeakTable<ITextSnapshot, SourceText> SnapshotMap = new();

        public static SourceTextContainer ToSourceTextContainer(this ITextBuffer textBuffer)
        {
            ArgumentNullException.ThrowIfNull(textBuffer);

            return ProviderMap.GetValue(textBuffer, tb => new VisualStudioSourceTextContainer(tb));
        }

        public static SourceText ToSourceText(this ITextSnapshot textSnapshot)
        {
            ArgumentNullException.ThrowIfNull(textSnapshot);

            var container = (VisualStudioSourceTextContainer)textSnapshot.TextBuffer.ToSourceTextContainer();
            return SnapshotMap.GetValue(textSnapshot, ts => new VisualStudioSourceText(container, ts));
        }

        public static ITextSnapshot ToTextSnapshot(this SourceText text)
        {
            ArgumentNullException.ThrowIfNull(text);

            if (text is not VisualStudioSourceText visualStudioSourceText)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromTextBuffer, nameof(text));

            return visualStudioSourceText.Snapshot;
        }

        public static ITextBuffer ToTextBuffer(this SourceTextContainer container)
        {
            ArgumentNullException.ThrowIfNull(container);

            if (container is not VisualStudioSourceTextContainer visualStudioSourceTextContainer)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromTextBuffer, nameof(container));

            return visualStudioSourceTextContainer.TextBuffer;
        }
    }
}