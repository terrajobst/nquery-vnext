using System;

namespace NQuery.BoundNodes
{
    internal enum BoundJoinType
    {
        InnerJoin,
        FullOuterJoin,
        LeftOuterJoin,
        RightOuterJoin
    }
}