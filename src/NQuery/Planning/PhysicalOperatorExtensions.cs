#nullable enable

namespace NQuery.Planning
{
    internal static class PhysicalOperatorExtensions
    {
        public static IEnumerable<PhysicalOperator> DescendantsAndSelf(this PhysicalOperator node)
        {
            yield return node;
            foreach (var input in node.GetInputs())
            {
                foreach (var descendant in input.DescendantsAndSelf())
                    yield return descendant;
            }
        }

        public static IEnumerable<PhysicalOperator> GetInputs(this PhysicalOperator node)
        {
            return node switch
            {
                PhysicalFilter n => [n.Input],
                PhysicalComputeScalar n => [n.Input],
                PhysicalProject n => [n.Input],
                PhysicalSort n => [n.Input],
                PhysicalTop n => [n.Input],
                PhysicalAggregate n => [n.Input],
                PhysicalJoin n => [n.Left, n.Right],
                PhysicalApply n => [n.Left, n.Right],
                PhysicalIntersectOrExcept n => [n.Left, n.Right],
                PhysicalConcatenation n => n.Inputs,
                _ => []
            };
        }
    }
}
