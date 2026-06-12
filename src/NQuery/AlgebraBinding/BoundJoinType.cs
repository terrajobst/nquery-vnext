using NQuery.Binding;

namespace NQuery.AlgebraBinding
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