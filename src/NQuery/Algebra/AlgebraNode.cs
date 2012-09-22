using System;

namespace NQuery.Algebra
{
    internal abstract class AlgebraNode
    {
        public abstract AlgebraKind Kind { get; }
    }
}