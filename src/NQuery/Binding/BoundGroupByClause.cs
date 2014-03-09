using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NQuery.Binding
{
    internal sealed class BoundGroupByClause
    {
        private readonly ReadOnlyCollection<ValueSlot> _columns;

        public BoundGroupByClause(IList<ValueSlot> columns)
        {
            _columns = new ReadOnlyCollection<ValueSlot>(columns);
        }

        public ReadOnlyCollection<ValueSlot> Columns
        {
            get { return _columns; }
        }
    }
}