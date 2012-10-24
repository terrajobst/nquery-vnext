using System;
using System.Collections.Generic;

namespace NQuery.Language.Services.Completion
{
    public interface ICompletionProvider
    {
        IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, int position);
    }
}