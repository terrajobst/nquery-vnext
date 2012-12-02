using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NQuery.Symbols;

namespace NQuery.BoundNodes
{
    internal sealed class BoundWildcardSelectColumn : BoundNode
    {
        private readonly TableInstanceSymbol _table;
        private readonly ReadOnlyCollection<ColumnInstanceSymbol> _columns;

        public BoundWildcardSelectColumn(TableInstanceSymbol table, IList<ColumnInstanceSymbol> columns)
        {
            _table = table;
            _columns = new ReadOnlyCollection<ColumnInstanceSymbol>(columns);
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.WildcardSelectColumn; }
        }

        public TableInstanceSymbol Table
        {
            get { return _table; }
        }

        public ReadOnlyCollection<ColumnInstanceSymbol> Columns
        {
            get { return _columns; }
        }
    }
}