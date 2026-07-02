namespace NQuery.CodeAnalysis.Planning;

internal static class PhysicalOperatorExtensions
{
    extension(PhysicalOperator node)
    {
        public IEnumerable<PhysicalOperator> DescendantsAndSelf()
        {
            yield return node;
            foreach (var input in node.GetInputs())
            {
                foreach (var descendant in input.DescendantsAndSelf())
                    yield return descendant;
            }
        }

        public IEnumerable<PhysicalOperator> GetInputs()
        {
            return node switch
            {
                PhysicalFilter n => [n.Input],
                PhysicalComputeScalar n => [n.Input],
                PhysicalProject n => [n.Input],
                PhysicalSort n => [n.Input],
                PhysicalTop n => [n.Input],
                PhysicalAssert n => [n.Input],
                PhysicalStreamAggregates n => [n.Input],
                PhysicalNestedLoops n => [n.Left, n.Right],
                PhysicalHashMatch n => [n.Build, n.Probe],
                PhysicalConcatenation n => n.Inputs,
                PhysicalRecursiveUnion n => [n.Anchor, .. n.RecursiveMembers],
                PhysicalIndexSpool n => [n.Input],
                _ => []
            };
        }
    }
}
