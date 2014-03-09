using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Binding
{
    internal sealed class BoundOrderByClause
    {
        private readonly ReadOnlyCollection<BoundOrderByColumn> _columns;

        public BoundOrderByClause(IList<BoundOrderByColumn> columns)
        {
            _columns = new ReadOnlyCollection<BoundOrderByColumn>(columns);
        }

        public ReadOnlyCollection<BoundOrderByColumn> Columns
        {
            get { return _columns; }
        }
    }
}