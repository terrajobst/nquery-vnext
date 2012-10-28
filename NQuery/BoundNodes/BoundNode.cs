using System;

namespace NQuery.BoundNodes
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
    }
}