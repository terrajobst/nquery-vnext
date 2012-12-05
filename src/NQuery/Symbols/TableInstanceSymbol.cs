using System;
using System.Collections.ObjectModel;
using System.Linq;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class TableInstanceSymbol : Symbol
    {
        private readonly TableSymbol _table;
        private readonly ReadOnlyCollection<TableColumnInstanceSymbol> _columnInstances;

        internal TableInstanceSymbol(string name, TableSymbol table, ValueSlotFactory valueFactory)
            : this(name, table, (ti, c) => valueFactory.CreateValueSlot(ti.Name + "." + c.Name, c.Type))
        {
        }

        internal TableInstanceSymbol(string name, TableSymbol table, Func<TableInstanceSymbol, ColumnSymbol, ValueSlot> valueFactory)
            : base(name)
        {
            _table = table;

            var columns = table.Columns.Select(c => new TableColumnInstanceSymbol(this, c, valueFactory)).ToArray();
            _columnInstances = new ReadOnlyCollection<TableColumnInstanceSymbol>(columns);
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.TableInstance; }
        }

        public TableSymbol Table
        {
            get { return _table; }
        }

        public ReadOnlyCollection<TableColumnInstanceSymbol> ColumnInstances
        {
            get { return _columnInstances; }
        }

        public override Type Type
        {
            get { return _table.Type; }
        }
    }
}