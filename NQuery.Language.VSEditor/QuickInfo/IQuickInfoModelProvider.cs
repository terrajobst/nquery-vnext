namespace NQuery.Language.VSEditor
{
    public interface IQuickInfoModelProvider
    {
        QuickInfoModel GetModel(SemanticModel semanticModel, int position);
    }
}