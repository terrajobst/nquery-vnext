namespace NQuery.Authoring.QuickInfo
{
    public interface IQuickInfoModelProvider
    {
        QuickInfoModel GetModel(SemanticModel semanticModel, int position);
    }
}