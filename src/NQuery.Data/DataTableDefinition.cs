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
        public DataTableDefinition(DataTable dataTable)
        {
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            DataTable = dataTable;
        }

        public DataTable DataTable { get; }

        public override string Name
        {
            get { return DataTable.TableName; }
        }

        public override Type RowType
        {
            get { return typeof (DataRow); }
        }

        protected override IEnumerable<ColumnDefinition> GetColumns()
        {
            return DataTable.Columns.Cast<DataColumn>().Select(c => new DataColumnDefinition(c));
        }

        public override IEnumerable GetRows()
        {
            return DataTable.Rows;
        }
    }
}