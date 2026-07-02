using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.Text;

using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.VSEditorWpf.Text;

public static class VisualStudioTextExtensions
{
    private static readonly ConditionalWeakTable<ITextBuffer, SourceTextContainer> ProviderMap = new();
    private static readonly ConditionalWeakTable<ITextSnapshot, SourceText> SnapshotMap = new();

    extension(ITextBuffer textBuffer)
    {
        public SourceTextContainer ToSourceTextContainer()
        {
            ThrowIfNull(textBuffer);

            return ProviderMap.GetValue(textBuffer, tb => new VisualStudioSourceTextContainer(tb));
        }
    }

    extension(ITextSnapshot textSnapshot)
    {
        public SourceText ToSourceText()
        {
            ThrowIfNull(textSnapshot);

            var container = (VisualStudioSourceTextContainer)textSnapshot.TextBuffer.ToSourceTextContainer();
            return SnapshotMap.GetValue(textSnapshot, ts => new VisualStudioSourceText(container, ts));
        }
    }

    extension(SourceText text)
    {
        public ITextSnapshot ToTextSnapshot()
        {
            ThrowIfNull(text);

            if (text is not VisualStudioSourceText visualStudioSourceText)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromTextBuffer, nameof(text));

            return visualStudioSourceText.Snapshot;
        }
    }

    extension(SourceTextContainer container)
    {
        public ITextBuffer ToTextBuffer()
        {
            ThrowIfNull(container);

            if (container is not VisualStudioSourceTextContainer visualStudioSourceTextContainer)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromTextBuffer, nameof(container));

            return visualStudioSourceTextContainer.TextBuffer;
        }
    }
}
