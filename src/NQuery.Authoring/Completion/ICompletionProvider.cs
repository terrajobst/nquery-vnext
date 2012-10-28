using System;
using System.Collections.Generic;

namespace NQuery.Authoring.Completion
{
    public interface ICompletionProvider
    {
        IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position);
    }
}