namespace NQuery.CodeAnalysis.Planning;

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
    RecursiveUnion,
    RecursiveReference,
    IndexSpool,
    Assert
}
