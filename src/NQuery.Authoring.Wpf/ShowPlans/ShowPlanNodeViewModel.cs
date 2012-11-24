using System;
using System.Collections.Generic;
using System.Linq;

namespace NQuery.Authoring.Wpf
{
    internal sealed class ShowPlanNodeViewModel : ShowPlanItemViewModel
    {
        private readonly ShowPlanNode _model;
        private readonly IEnumerable<ShowPlanItemViewModel> _children;

        public ShowPlanNodeViewModel(ShowPlanNode model)
        {
            var propertyChildren = model.Properties.Select(p => new ShowPlanPropertyViewModel(p));
            var nodeChildren = model.Children.Select(c => new ShowPlanNodeViewModel(c));

            _model = model;
            _children = propertyChildren.Concat<ShowPlanItemViewModel>(nodeChildren).ToArray();

        }

        public override string DisplayName
        {
            get { return _model.OperatorName; }
        }

        public override IEnumerable<ShowPlanItemViewModel> Children
        {
            get { return _children; }
        }

        public override string Kind
        {
            get { return _model.IsScalar ? "Scalar" : "Relational"; }
        }
    }
}