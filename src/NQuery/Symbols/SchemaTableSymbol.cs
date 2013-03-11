using System;
using System.Collections.Generic;
using System.Linq;

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

    public sealed class SchemaTableSymbol : TableSymbol
    {
        private readonly TableDefinition _tableDefinition;

        public SchemaTableSymbol(TableDefinition tableDefinition)
            : base(GetName(tableDefinition), GetColumns(tableDefinition))
        {
            _tableDefinition = tableDefinition;
        }

        private static string GetName(TableDefinition tableDefinition)
        {
            if (tableDefinition == null)
                throw new ArgumentNullException("tableDefinition");

            return tableDefinition.Name;
        }

        private static IList<ColumnSymbol> GetColumns(TableDefinition tableDefinition)
        {
            if (tableDefinition == null)
                throw new ArgumentNullException("tableDefinition");

            return tableDefinition.Columns.Select(c => new SchemaColumnSymbol(c)).ToArray();
        }

        public TableDefinition Definition
        {
            get { return _tableDefinition; }
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.SchemaTable; }
        }

        public override Type Type
        {
            get { return _tableDefinition.RowType; }
        }
    }
}