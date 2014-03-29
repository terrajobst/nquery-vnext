using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundWildcardSelectColumn : BoundNode
    {
        private readonly TableInstanceSymbol _table;
        private readonly ImmutableArray<TableColumnInstanceSymbol> _tableColumns;
        private readonly ImmutableArray<QueryColumnInstanceSymbol> _queryColumns;

        public BoundWildcardSelectColumn(TableInstanceSymbol table, IEnumerable<TableColumnInstanceSymbol> columns)
        {
            _table = table;
            _tableColumns = columns.ToImmutableArray();
            _queryColumns = _tableColumns.Select(c => new QueryColumnInstanceSymbol(c.Name, c.ValueSlot)).ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.WildcardSelectColumn; }
        }

        public TableInstanceSymbol Table
        {
            get { return _table; }
        }

        public ImmutableArray<TableColumnInstanceSymbol> TableColumns
        {
            get { return _tableColumns; }
        }

        public ImmutableArray<QueryColumnInstanceSymbol> QueryColumns
        {
            get { return _queryColumns; }
        }
    }
}