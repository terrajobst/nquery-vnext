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
        Join,
        Apply,
        Aggregate,
        Sort,
        Top,
        Concatenation,
        IntersectOrExcept
    }
}
