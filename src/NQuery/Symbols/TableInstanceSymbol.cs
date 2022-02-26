using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Symbols
{
    public sealed class TableInstanceSymbol : Symbol
    {
        internal TableInstanceSymbol(string name, TableSymbol table, ValueSlotFactory valueFactory)
            : this(name, table, (ti, c) => valueFactory.CreateNamed($"{ti.Name}.{c.Name}", c.Type))
        {
        }

        internal TableInstanceSymbol(string name, TableSymbol table, Func<TableInstanceSymbol, ColumnSymbol, ValueSlot> valueFactory)
            : base(name)
        {
            Table = table;
            ColumnInstances = table.Columns.Select(c => new TableColumnInstanceSymbol(this, c, valueFactory)).ToImmutableArray();
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.TableInstance; }
        }

        public TableSymbol Table { get; }

        public ImmutableArray<TableColumnInstanceSymbol> ColumnInstances { get; }

        public override Type Type
        {
            get { return Table.Type; }
        }
    }
}