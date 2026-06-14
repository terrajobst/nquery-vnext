#nullable enable

namespace NQuery.Algebra;

internal enum LogicalOperatorKind
{
    Empty,
    Constant,
    TableScan,
    Filter,
    Compute,
    Project,
    Join,
    Apply,
    Aggregate,
    Union,
    IntersectOrExcept,
    Sort,
    Top,
    Assert
}
