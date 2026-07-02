namespace NQuery.CodeAnalysis.Algebra;

internal static class LogicalOperatorExtensions
{
    extension(LogicalOperator node)
    {
        public IEnumerable<LogicalOperator> DescendantsAndSelf()
        {
            yield return node;
            foreach (var input in node.GetInputs())
            {
                foreach (var descendant in input.DescendantsAndSelf())
                    yield return descendant;
            }
        }

        public IEnumerable<LogicalOperator> GetInputs()
        {
            return node switch
            {
                LogicalFilter n => [n.Input],
                LogicalCompute n => [n.Input],
                LogicalProject n => [n.Input],
                LogicalSort n => [n.Input],
                LogicalTop n => [n.Input],
                LogicalAssert n => [n.Input],
                LogicalAggregate n => [n.Input],
                LogicalJoin n => [n.Left, n.Right],
                LogicalApply n => [n.Left, n.Right],
                LogicalIntersectOrExcept n => [n.Left, n.Right],
                LogicalUnion n => n.Inputs,
                LogicalRecursiveUnion n => [n.Anchor, .. n.RecursiveMembers],
                _ => []
            };
        }
    }
}
