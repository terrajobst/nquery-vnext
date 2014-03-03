using System;

namespace NQuery
{
    public sealed class ShowPlan
    {
        private readonly string _name;
        private readonly ShowPlanNode _root;

        public ShowPlan(string name, ShowPlanNode root)
        {
            _name = name;
            _root = root;
        }

        public string Name
        {
            get { return _name; }
        }

        public ShowPlanNode Root
        {
            get { return _root; }
        }
    }
}