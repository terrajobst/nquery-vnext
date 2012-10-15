using System.Collections.Generic;

namespace NQuery.Language.VSEditor.Completion
{
    public interface ICompletionProvider
    {
        IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position);
    }
}