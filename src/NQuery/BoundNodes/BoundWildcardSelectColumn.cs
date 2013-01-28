using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundWildcardSelectColumn : BoundNode
    {
        private readonly TableInstanceSymbol _table;
        private readonly ReadOnlyCollection<TableColumnInstanceSymbol> _tableColumns;
        private readonly ReadOnlyCollection<QueryColumnInstanceSymbol> _queryColumns;

        public BoundWildcardSelectColumn(TableInstanceSymbol table, IList<TableColumnInstanceSymbol> columns)
        {
            var queryColumns = columns.Select(c => new QueryColumnInstanceSymbol(c.Name, c.ValueSlot)).ToArray();

            _table = table;
            _tableColumns = new ReadOnlyCollection<TableColumnInstanceSymbol>(columns);
            _queryColumns =new ReadOnlyCollection<QueryColumnInstanceSymbol>(queryColumns);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.WildcardSelectColumn; }
        }

        public TableInstanceSymbol Table
        {
            get { return _table; }
        }

        public ReadOnlyCollection<TableColumnInstanceSymbol> TableColumns
        {
            get { return _tableColumns; }
        }

        public ReadOnlyCollection<QueryColumnInstanceSymbol> QueryColumns
        {
            get { return _queryColumns; }
        }
    }
}