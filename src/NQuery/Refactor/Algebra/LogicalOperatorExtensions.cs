#nullable enable

namespace NQuery.Refactor.Algebra
{
    internal static class LogicalOperatorExtensions
    {
        public static IEnumerable<LogicalOperator> DescendantsAndSelf(this LogicalOperator node)
        {
            yield return node;
            foreach (var input in node.GetInputs())
            {
                foreach (var descendant in input.DescendantsAndSelf())
                    yield return descendant;
            }
        }

        public static IEnumerable<LogicalOperator> GetInputs(this LogicalOperator node)
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
                _ => []
            };
        }
    }
}
