using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace NQuery.Symbols
{
    public sealed class TableInstanceSymbol : Symbol
    {
        private readonly TableSymbol _table;
        private readonly ReadOnlyCollection<ColumnInstanceSymbol> _columnInstances;

        public TableInstanceSymbol(string name, TableSymbol table)
            : base(name)
        {
            _table = table;

            var columns = table.Columns.Select(c => new ColumnInstanceSymbol(this, c)).ToArray();
            _columnInstances = new ReadOnlyCollection<ColumnInstanceSymbol>(columns);
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.TableInstance; }
        }

        public TableSymbol Table
        {
            get { return _table; }
        }

        public ReadOnlyCollection<ColumnInstanceSymbol> ColumnInstances
        {
            get { return _columnInstances; }
        }

        public override Type Type
        {
            get { return _table.Type; }
        }
    }
}