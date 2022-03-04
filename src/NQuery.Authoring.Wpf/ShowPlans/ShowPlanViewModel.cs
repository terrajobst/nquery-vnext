namespace NQuery.Authoring.Wpf
{
    internal sealed class ShowPlanViewModel
    {
        public ShowPlanViewModel(ShowPlanNode model)
        {
            Model = model;
            Root = new[] { new ShowPlanNodeViewModel(model) };
        }

        public ShowPlanNode Model { get; }

        public ShowPlanNodeViewModel[] Root { get; }
    }
}