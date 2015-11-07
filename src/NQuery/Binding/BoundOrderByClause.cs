using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundOrderByClause
    {
        public BoundOrderByClause(IEnumerable<BoundOrderByColumn> columns)
        {
            Columns = columns.ToImmutableArray();
        }

        public ImmutableArray<BoundOrderByColumn> Columns { get; }
    }
}