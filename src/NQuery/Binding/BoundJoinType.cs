using NQuery.Binding;

namespace NQuery.Binding
{
    internal enum BoundJoinType
    {
        Inner,
        FullOuter,
        LeftOuter,
        RightOuter,
        LeftSemi,
        LeftAntiSemi
    }
}