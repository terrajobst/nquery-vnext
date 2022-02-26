namespace NQuery.Authoring.Wpf
{
    internal sealed class ShowPlanPropertyViewModel : ShowPlanItemViewModel
    {
        private readonly KeyValuePair<string, string> _model;

        public ShowPlanPropertyViewModel(KeyValuePair<string, string> model)
        {
            _model = model;
        }

        public override string DisplayName
        {
            get { return $"{_model.Key} = {_model.Value}"; }
        }

        public override IEnumerable<ShowPlanItemViewModel> Children
        {
            get { return Enumerable.Empty<ShowPlanItemViewModel>(); }
        }

        public override string Kind
        {
            get { return @"Property"; }
        }
    }
}