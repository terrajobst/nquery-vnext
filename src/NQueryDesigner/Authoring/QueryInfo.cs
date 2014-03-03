using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Symbols;

namespace NQueryDesigner.Authoring
{
    public sealed class QueryInfo
    {
        private readonly ReadOnlyCollection<SelectionInfo> _selectionInfos;
        private readonly ReadOnlyCollection<TableInstanceSymbol> _tables;

        public QueryInfo(IEnumerable<SelectionInfo> selectionInfos, IEnumerable<TableInstanceSymbol> tables)
        {
            _selectionInfos = new ReadOnlyCollection<SelectionInfo>(selectionInfos.ToArray());
            _tables = new ReadOnlyCollection<TableInstanceSymbol>(tables.ToArray());
        }

        public ReadOnlyCollection<SelectionInfo> SelectionInfos
        {
            get { return _selectionInfos; }
        }

        public ReadOnlyCollection<TableInstanceSymbol> Tables
        {
            get { return _tables; }
        }
    }
}