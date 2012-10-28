using System;
using System.Collections.ObjectModel;

namespace NQuery.BoundNodes
{
    internal abstract class BoundQuery : BoundNode
    {
        public abstract ReadOnlyCollection<BoundSelectColumn> SelectColumns { get; }
    }
}