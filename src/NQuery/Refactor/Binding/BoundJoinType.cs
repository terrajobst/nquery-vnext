using NQuery.Binding;

namespace NQuery.Refactor.Binding
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