using System.Collections;
using System.Collections.Immutable;
using System.Data;

using NQuery.Symbols;

namespace NQuery.Data
{
    public sealed class DataTableSymbol : SchemaTableSymbol
    {
        private readonly DataTable _dataTable;

        public DataTableSymbol(DataTable dataTable)
            : base(dataTable?.TableName)
        {
            ArgumentNullException.ThrowIfNull(dataTable);

            _dataTable = dataTable;
            Columns = _dataTable.Columns.Cast<DataColumn>().Select(c => (ColumnSymbol)new DataColumnSymbol(c)).ToImmutableArray();
        }

        public DataTableSymbol(DataTable dataTable, string name)
            : base(name)
        {
            ArgumentNullException.ThrowIfNull(dataTable);

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(@"The table name must not be null or empty.", nameof(name));

            _dataTable = dataTable;
        }

        public override Type Type
        {
            get { return typeof(DataRow); }
        }

        public override ImmutableArray<ColumnSymbol> Columns { get; }

        public override IEnumerable GetRows()
        {
            return _dataTable.Rows;
        }
    }
}