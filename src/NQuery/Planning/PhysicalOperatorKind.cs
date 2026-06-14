#nullable enable

namespace NQuery.Planning;

internal enum PhysicalOperatorKind
{
    Empty,
    Constant,
    TableScan,
    Filter,
    ComputeScalar,
    Project,
    NestedLoops,
    HashMatch,
    StreamAggregates,
    Sort,
    Top,
    Concatenation,
    Assert
}
