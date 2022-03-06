using System.Runtime.CompilerServices;

using ActiproSoftware.Text;

using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Text
{
    public static class ActiproTextExtensions
    {
        private static readonly ConditionalWeakTable<ITextDocument, SourceTextContainer> ProviderMap = new();
        private static readonly ConditionalWeakTable<ITextSnapshot, SourceText> SnapshotMap = new();

        public static SourceTextContainer ToSourceTextContainer(this ITextDocument textDocument)
        {
            ArgumentNullException.ThrowIfNull(textDocument);

            return ProviderMap.GetValue(textDocument, tb => new ActiproSourceTextContainer(tb));
        }

        public static SourceText ToSourceText(this ITextSnapshot textSnapshot)
        {
            ArgumentNullException.ThrowIfNull(textSnapshot);

            var container = (ActiproSourceTextContainer)textSnapshot.Document.ToSourceTextContainer();
            return SnapshotMap.GetValue(textSnapshot, ts => new ActiproSourceText(container, ts));
        }

        public static ITextSnapshot ToTextSnapshot(this SourceText text)
        {
            ArgumentNullException.ThrowIfNull(text);

            if (text is not ActiproSourceText actiproSourceText)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromActiproEditor, nameof(text));

            return actiproSourceText.Snapshot;
        }

        public static ITextDocument ToTextDocument(this SourceTextContainer container)
        {
            ArgumentNullException.ThrowIfNull(container);

            if (container is not ActiproSourceTextContainer actiproSourceTextContainer)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromActiproEditor, nameof(container));

            return actiproSourceTextContainer.TextDocument;
        }
    }
}