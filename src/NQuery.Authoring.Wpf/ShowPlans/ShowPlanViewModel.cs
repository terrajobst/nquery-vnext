using System;

namespace NQuery.Authoring.Wpf
{
    internal sealed class ShowPlanViewModel
    {
        private readonly ShowPlanNode _model;
        private readonly ShowPlanNodeViewModel[] _root;

        public ShowPlanViewModel(ShowPlanNode model)
        {
            _model = model;
            _root = new[] {new ShowPlanNodeViewModel(model)};
        }

        public ShowPlanNode Model
        {
            get { return _model; }
        }

        public ShowPlanNodeViewModel[] Root
        {
            get { return _root; }
        }
    }
}