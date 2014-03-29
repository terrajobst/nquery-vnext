using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundGroupByClause
    {
        private readonly ImmutableArray<ValueSlot> _columns;

        public BoundGroupByClause(IEnumerable<ValueSlot> columns)
        {
            _columns = columns.ToImmutableArray();
        }

        public ImmutableArray<ValueSlot> Columns
        {
            get { return _columns; }
        }
    }
}