#nullable enable

namespace NQuery.Planning
{
    internal enum PhysicalOperatorKind
    {
        Empty,
        Constant,
        TableScan,
        Filter,
        ComputeScalar,
        Project,
        NestedLoops,
        Aggregate,
        Sort,
        Top,
        Concatenation,
        IntersectOrExcept
    }
}
