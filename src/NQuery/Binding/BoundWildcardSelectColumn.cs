using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Binding
{
    internal sealed class BoundWildcardSelectColumn : BoundNode
    {
        private readonly ImmutableArray<TableColumnInstanceSymbol> _tableColumns;

        public BoundWildcardSelectColumn(TableInstanceSymbol table, IEnumerable<TableColumnInstanceSymbol> columns)
        {
            Table = table;
            _tableColumns = columns.ToImmutableArray();
            QueryColumns = _tableColumns.Select(c => new QueryColumnInstanceSymbol(c.Name, c.ValueSlot)).ToImmutableArray();
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.WildcardSelectColumn; }
        }

        public TableInstanceSymbol Table { get; }

        public ImmutableArray<TableColumnInstanceSymbol> TableColumns
        {
            get { return _tableColumns; }
        }

        public ImmutableArray<QueryColumnInstanceSymbol> QueryColumns { get; }
    }
}