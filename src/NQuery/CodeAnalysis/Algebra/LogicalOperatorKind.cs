namespace NQuery.CodeAnalysis.Algebra;

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
    RecursiveUnion,
    RecursiveReference,
    IntersectOrExcept,
    Sort,
    Top,
    Assert
}
