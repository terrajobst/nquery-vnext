namespace NQuery.Authoring.Wpf;

internal sealed class ShowPlanViewModel
{
    public ShowPlanViewModel(ShowPlanNode model)
    {
        ThrowIfNull(model);

        Model = model;
        Root = [new ShowPlanNodeViewModel(model)];
    }

    public ShowPlanNode Model { get; }

    public ShowPlanNodeViewModel[] Root { get; }
}
