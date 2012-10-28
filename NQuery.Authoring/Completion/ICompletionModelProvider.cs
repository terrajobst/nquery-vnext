using System;

namespace NQuery.Authoring.Completion
{
    public interface ICompletionModelProvider
    {
        CompletionModel GetModel(SemanticModel semanticModel, int position);
    }
}