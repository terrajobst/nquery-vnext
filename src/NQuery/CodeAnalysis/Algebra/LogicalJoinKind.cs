namespace NQuery.CodeAnalysis.Algebra;

// The logical layer's closed set of join kinds. Note there is no RightOuter:
// the algebrizer normalizes RIGHT OUTER to LEFT OUTER by swapping inputs, so a
// right-outer join is unrepresentable here. FullOuter stays because it is not
// symmetric.
internal enum LogicalJoinKind
{
    Inner,
    LeftOuter,
    FullOuter,
    LeftSemi,
    LeftAntiSemi
}
