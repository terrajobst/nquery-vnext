using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using NQuery.Symbols;

namespace NQuery.Data
{
    public sealed class DataTableDefinition : TableDefinition
    {
        private readonly DataTable _dataTable;

        public DataTableDefinition(DataTable dataTable)
        {
            _dataTable = dataTable;
        }

        public DataTable DataTable
        {
            get { return _dataTable; }
        }

        public override string Name
        {
            get { return _dataTable.TableName; }
        }

        public override Type RowType
        {
            get { return typeof (DataRow); }
        }

        protected override IEnumerable<ColumnDefinition> GetColumns()
        {
            return _dataTable.Columns.Cast<DataColumn>().Select(c => new DataColumnDefinition(c));
        }

        public override IEnumerable GetRows()
        {
            return _dataTable.Rows;
        }
    }
}