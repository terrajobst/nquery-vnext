using System;

namespace NQueryDesigner.Authoring
{
    public sealed class SelectionInfo
    {
        private readonly string _column;
        private readonly string _alias;
        private readonly string _table;
        private readonly string _aggregate;
        private readonly QuerySortOrder _sortOrder;

        public SelectionInfo(string column, string alias, string table, string aggregate, QuerySortOrder sortOrder)
        {
            _column = column;
            _alias = alias;
            _table = table;
            _aggregate = aggregate;
            _sortOrder = sortOrder;
        }

        public string Column
        {
            get { return _column; }
        }

        public string Alias
        {
            get { return _alias; }
        }

        public string Table
        {
            get { return _table; }
        }

        public string Aggregate
        {
            get { return _aggregate; }
        }

        public QuerySortOrder SortOrder
        {
            get { return _sortOrder; }
        }
    }
}