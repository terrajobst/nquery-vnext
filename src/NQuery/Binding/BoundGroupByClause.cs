using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundGroupByClause
    {
        public BoundGroupByClause(IEnumerable<ValueSlot> columns)
        {
            Columns = columns.ToImmutableArray();
        }

        public ImmutableArray<ValueSlot> Columns { get; }
    }
}