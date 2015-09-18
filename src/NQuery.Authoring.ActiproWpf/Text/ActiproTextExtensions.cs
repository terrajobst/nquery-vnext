using System;
using System.Runtime.CompilerServices;

using ActiproSoftware.Text;

using NQuery.Text;

namespace NQuery.Authoring.ActiproWpf.Text
{
    public static class ActiproTextExtensions
    {
        private static readonly ConditionalWeakTable<ITextDocument, SourceTextContainer> ProviderMap = new ConditionalWeakTable<ITextDocument, SourceTextContainer>();
        private static readonly ConditionalWeakTable<ITextSnapshot, SourceText> SnapshotMap = new ConditionalWeakTable<ITextSnapshot, SourceText>();

        public static SourceTextContainer ToSourceTextContainer(this ITextDocument textDocument)
        {
            if (textDocument == null)
                throw new ArgumentNullException(nameof(textDocument));

            return ProviderMap.GetValue(textDocument, tb => new ActiproSourceTextContainer(tb));
        }

        public static SourceText ToSourceText(this ITextSnapshot textSnapshot)
        {
            if (textSnapshot == null)
                throw new ArgumentNullException(nameof(textSnapshot));

            var container = (ActiproSourceTextContainer)textSnapshot.Document.ToSourceTextContainer();
            return SnapshotMap.GetValue(textSnapshot, ts => new ActiproSourceText(container, ts));
        }

        public static ITextSnapshot ToTextSnapshot(this SourceText text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var actiproSourceText = text as ActiproSourceText;
            if (actiproSourceText == null)
                throw new ArgumentException("The source text didn't originate from an Actipro Syntax Editor", nameof(text));

            return actiproSourceText.Snapshot;
        }

        public static ITextDocument ToTextDocument(this SourceTextContainer container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            var actiproSourceTextContainer = container as ActiproSourceTextContainer;
            if (actiproSourceTextContainer == null)
                throw new ArgumentException("The source text container didn't originate from an Actipro Syntax Editor", nameof(container));

            return actiproSourceTextContainer.TextDocument;
        }
    }
}