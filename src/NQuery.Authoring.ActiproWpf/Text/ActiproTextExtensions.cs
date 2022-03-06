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
            if (textDocument is null)
                throw new ArgumentNullException(nameof(textDocument));

            return ProviderMap.GetValue(textDocument, tb => new ActiproSourceTextContainer(tb));
        }

        public static SourceText ToSourceText(this ITextSnapshot textSnapshot)
        {
            if (textSnapshot is null)
                throw new ArgumentNullException(nameof(textSnapshot));

            var container = (ActiproSourceTextContainer)textSnapshot.Document.ToSourceTextContainer();
            return SnapshotMap.GetValue(textSnapshot, ts => new ActiproSourceText(container, ts));
        }

        public static ITextSnapshot ToTextSnapshot(this SourceText text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            var actiproSourceText = text as ActiproSourceText;
            if (actiproSourceText is null)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromActiproEditor, nameof(text));

            return actiproSourceText.Snapshot;
        }

        public static ITextDocument ToTextDocument(this SourceTextContainer container)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));

            var actiproSourceTextContainer = container as ActiproSourceTextContainer;
            if (actiproSourceTextContainer is null)
                throw new ArgumentException(Resources.SourceTextMustOriginateFromActiproEditor, nameof(container));

            return actiproSourceTextContainer.TextDocument;
        }
    }
}