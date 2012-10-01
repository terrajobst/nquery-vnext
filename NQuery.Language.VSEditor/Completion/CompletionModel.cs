using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Language.VSEditor.Completion
{
    public sealed class CompletionModel
    {
        public CompletionModel(TextSpan applicableSpan, IList<CompletionItem> items)
        {
            ApplicableSpan = applicableSpan;
            Items = new ReadOnlyCollection<CompletionItem>(items);
        }

        public TextSpan ApplicableSpan { get; private set; }
        public ReadOnlyCollection<CompletionItem> Items { get; private set; }
    }
}