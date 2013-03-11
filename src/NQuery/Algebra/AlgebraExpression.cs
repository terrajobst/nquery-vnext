using System;

namespace NQuery.Algebra
{
    internal abstract class AlgebraExpression : AlgebraNode
    {
        public abstract Type Type { get; }
    }
}