using System;

namespace NQuery.Symbols
{
    public sealed class SchemaColumnSymbol : ColumnSymbol
    {
        private readonly ColumnDefinition _columnDefinition;

        public SchemaColumnSymbol(ColumnDefinition columnDefinition)
            : base(columnDefinition.Name, columnDefinition.DataType)
        {
            _columnDefinition = columnDefinition;
        }

        public ColumnDefinition Definition
        {
            get { return _columnDefinition; }
        }
    }
}