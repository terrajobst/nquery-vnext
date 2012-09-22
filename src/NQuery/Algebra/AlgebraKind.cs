using System;

namespace NQuery.Algebra
{
    internal enum AlgebraKind
    {
        Constant,
        Table,
        Join,
        Filter,
        Compute,
        Top,
        Concat
    }
}