namespace NQuery
{
    public sealed class ShowPlan
    {
        internal ShowPlan(string name, ShowPlanNode root)
        {
            Name = name;
            Root = root;
        }

        public string Name { get; }

        public ShowPlanNode Root { get; }
    }
}