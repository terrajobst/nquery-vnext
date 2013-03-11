using System;
using System.Data;
using System.Data.SqlTypes;

using NQuery.Symbols;

namespace NQuery.Data
{
    public sealed class DataColumnDefinition : ColumnDefinition
    {
        private readonly DataColumn _dataColumn;

        public DataColumnDefinition(DataColumn dataColumn)
        {
            _dataColumn = dataColumn;
        }

        public DataColumn DataColumn
        {
            get { return _dataColumn; }
        }

        public override string Name
        {
            get { return _dataColumn.ColumnName; }
        }

        public override Type DataType
        {
            get { return _dataColumn.DataType; }
        }

        public override object GetValue(object row)
        {
            var dataRow = (DataRow) row;
            var value = dataRow[_dataColumn];

            if (value is DBNull)
                return null;

            var nullable = value as INullable;
            if (nullable != null && nullable.IsNull)
                return null;

            return value;
        }
    }
}