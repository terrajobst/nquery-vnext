﻿using System.Runtime.CompilerServices;

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
            if (textBuffer is null)
                throw new ArgumentNullException(nameof(textBuffer));

            return ProviderMap.GetValue(textBuffer, tb => new VisualStudioSourceTextContainer(tb));
        }

        public static SourceText ToSourceText(this ITextSnapshot textSnapshot)
        {
            if (textSnapshot is null)
                throw new ArgumentNullException(nameof(textSnapshot));

            var container = (VisualStudioSourceTextContainer)textSnapshot.TextBuffer.ToSourceTextContainer();
            return SnapshotMap.GetValue(textSnapshot, ts => new VisualStudioSourceText(container, ts));
        }

        public static ITextSnapshot ToTextSnapshot(this SourceText text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var visualStudioSourceText = text as VisualStudioSourceText;
            if (visualStudioSourceText is null)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromTextBuffer, nameof(text));

            return visualStudioSourceText.Snapshot;
        }

        public static ITextBuffer ToTextBuffer(this SourceTextContainer container)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));

            var visualStudioSourceTextContainer = container as VisualStudioSourceTextContainer;
            if (visualStudioSourceTextContainer is null)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromTextBuffer, nameof(container));

            return visualStudioSourceTextContainer.TextBuffer;
        }
    }
}