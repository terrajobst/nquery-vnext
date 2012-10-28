using System;

namespace NQuery.Language.Services.Completion
{
    public interface ICompletionModelProvider
    {
        CompletionModel GetModel(SemanticModel semanticModel, int position);
    }
}