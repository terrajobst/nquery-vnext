using System;

namespace NQuery.Language.Symbols
{
    public sealed class ColumnInstanceSymbol : Symbol
    {
        private readonly TableInstanceSymbol _tableInstance;
        private readonly ColumnSymbol _column;

        public ColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column)
            : base(column.Name)
        {
            _tableInstance = tableInstance;
            _column = column;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.ColumnInstance; }
        }

        public TableInstanceSymbol TableInstance
        {
            get { return _tableInstance; }
        }

        public ColumnSymbol Column
        {
            get { return _column; }
        }

        public override Type Type
        {
            get { return _column.Type; }
        }
    }
}