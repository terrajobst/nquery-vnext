using System;

namespace NQuery.Language.Services.QuickInfo
{
    public interface IQuickInfoModelProvider
    {
        QuickInfoModel GetModel(SemanticModel semanticModel, int position);
    }
}