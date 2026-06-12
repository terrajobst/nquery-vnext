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

        // Refactor pipeline, real table: each column is its own value identity (the algebrizer
        // mints its slot at the scan).
        internal TableInstanceSymbol(string name, TableSymbol table)
            : base(name)
        {
            Table = table;
            ColumnInstances = table.Columns.Select(c => new TableColumnInstanceSymbol(this, c)).ToImmutableArray();
        }

        // Refactor pipeline, derived table: each column aliases the inner query's value.
        internal TableInstanceSymbol(string name, TableSymbol table, Func<TableInstanceSymbol, ColumnSymbol, NQuery.Refactor.Binding.IBoundValue> aliasFactory)
            : base(name)
        {
            Table = table;
            ColumnInstances = table.Columns.Select(c => new TableColumnInstanceSymbol(this, c, aliasFactory(this, c))).ToImmutableArray();
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