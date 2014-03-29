using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NQuery.Binding
{
    internal sealed class BoundOrderByClause
    {
        private readonly ImmutableArray<BoundOrderByColumn> _columns;

        public BoundOrderByClause(IEnumerable<BoundOrderByColumn> columns)
        {
            _columns = columns.ToImmutableArray();
        }

        public ImmutableArray<BoundOrderByColumn> Columns
        {
            get { return _columns; }
        }
    }
}