using System;

namespace NQuery.Language.Symbols
{
    public sealed class TableInstanceSymbol : Symbol
    {
        private readonly TableSymbol _table;

        public TableInstanceSymbol(string name, TableSymbol table)
            : base(name)
        {
            _table = table;
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.TableInstance; }
        }

        public TableSymbol Table
        {
            get { return _table; }
        }

        public override Type Type
        {
            get { return _table.Type; }
        }

        public override string ToString()
        {
            return string.Format("ROW {0} ({1})", Name, _table);
        }
    }
}