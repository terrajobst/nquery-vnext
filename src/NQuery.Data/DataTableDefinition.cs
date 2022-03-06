using System.Collections;
using System.Data;

using NQuery.Symbols;

namespace NQuery.Data
{
    public sealed class DataTableDefinition : TableDefinition
    {
        public DataTableDefinition(DataTable dataTable)
        {
            ArgumentNullException.ThrowIfNull(dataTable);

            DataTable = dataTable;
            Name = DataTable.TableName;
        }

        public DataTableDefinition(DataTable dataTable, string name)
        {
            ArgumentNullException.ThrowIfNull(dataTable);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"The table name must not be null or empty.", nameof(name));

            DataTable = dataTable;
            Name = name;
        }

        public DataTable DataTable { get; }

        public override string Name { get; }

        public override Type RowType
        {
            get { return typeof(DataRow); }
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