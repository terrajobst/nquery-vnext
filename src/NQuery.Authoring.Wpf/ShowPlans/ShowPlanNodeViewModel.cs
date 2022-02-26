using System.Collections.Immutable;

namespace NQuery.Authoring.Wpf
{
    internal sealed class ShowPlanNodeViewModel : ShowPlanItemViewModel
    {
        private readonly ShowPlanNode _model;

        public ShowPlanNodeViewModel(ShowPlanNode model)
        {
            var propertyChildren = model.Properties.Select(p => new ShowPlanPropertyViewModel(p));
            var nodeChildren = model.Children.Select(c => new ShowPlanNodeViewModel(c));

            _model = model;
            Children = propertyChildren.Concat<ShowPlanItemViewModel>(nodeChildren).ToImmutableArray();
        }

        public override string DisplayName
        {
            get { return _model.OperatorName; }
        }

        public override IEnumerable<ShowPlanItemViewModel> Children { get; }

        public override string Kind
        {
            get { return _model.IsScalar ? @"Scalar" : @"Relational"; }
        }
    }
}