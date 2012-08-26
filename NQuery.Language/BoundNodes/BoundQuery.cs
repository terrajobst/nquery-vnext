using System.Collections.ObjectModel;

namespace NQuery.Language.BoundNodes
{
    internal abstract class BoundQuery : BoundNode
    {
        public abstract ReadOnlyCollection<BoundSelectColumn> SelectColumns { get; }
    }
}