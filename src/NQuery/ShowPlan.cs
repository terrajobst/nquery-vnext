using System;

namespace NQuery
{
    public sealed class ShowPlan
    {
        public ShowPlan(string name, ShowPlanNode root)
        {
            Name = name;
            Root = root;
        }

        public string Name { get; }

        public ShowPlanNode Root { get; }
    }
}