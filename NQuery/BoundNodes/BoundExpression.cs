using System;

namespace NQuery.BoundNodes
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }
}