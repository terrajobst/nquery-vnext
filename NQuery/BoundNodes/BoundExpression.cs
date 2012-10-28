using System;

namespace NQuery.Language.BoundNodes
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }
}