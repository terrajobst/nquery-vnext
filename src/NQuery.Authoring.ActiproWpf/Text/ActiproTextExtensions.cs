using System.Runtime.CompilerServices;

using ActiproSoftware.Text;

using NQuery.CodeAnalysis.Text;

namespace NQuery.Authoring.ActiproWpf.Text;

public static class ActiproTextExtensions
{
    private static readonly ConditionalWeakTable<ITextDocument, SourceTextContainer> ProviderMap = new();
    private static readonly ConditionalWeakTable<ITextSnapshot, SourceText> SnapshotMap = new();

    extension(ITextDocument textDocument)
    {
        public SourceTextContainer ToSourceTextContainer()
        {
            ThrowIfNull(textDocument);

            return ProviderMap.GetValue(textDocument, tb => new ActiproSourceTextContainer(tb));
        }
    }

    extension(ITextSnapshot textSnapshot)
    {
        public SourceText ToSourceText()
        {
            ThrowIfNull(textSnapshot);

            var container = (ActiproSourceTextContainer)textSnapshot.Document.ToSourceTextContainer();
            return SnapshotMap.GetValue(textSnapshot, ts => new ActiproSourceText(container, ts));
        }
    }

    extension(SourceText text)
    {
        public ITextSnapshot ToTextSnapshot()
        {
            ThrowIfNull(text);

            if (text is not ActiproSourceText actiproSourceText)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromActiproEditor, nameof(text));

            return actiproSourceText.Snapshot;
        }
    }

    extension(SourceTextContainer container)
    {
        public ITextDocument ToTextDocument()
        {
            ThrowIfNull(container);

            if (container is not ActiproSourceTextContainer actiproSourceTextContainer)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromActiproEditor, nameof(container));

            return actiproSourceTextContainer.TextDocument;
        }
    }
}
