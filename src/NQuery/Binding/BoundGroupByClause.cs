using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundGroupByClause
    {
        public BoundGroupByClause(IEnumerable<BoundComparedValue> groups)
        {
            Groups = groups.ToImmutableArray();
        }

        public ImmutableArray<BoundComparedValue> Groups { get; }
    }
}