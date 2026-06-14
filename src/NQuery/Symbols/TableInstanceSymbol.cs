using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Symbols;

public sealed class TableInstanceSymbol : Symbol
{
    // Real table: each column is its own value identity (the algebrizer mints its slot at the
    // scan).
    internal TableInstanceSymbol(string name, TableSymbol table)
        : base(name)
    {
        Table = table;
        ColumnInstances = table.Columns.Select(c => new TableColumnInstanceSymbol(this, c)).ToImmutableArray();
    }

    // Derived table: each column aliases the inner query's value.
    internal TableInstanceSymbol(string name, TableSymbol table, Func<TableInstanceSymbol, ColumnSymbol, IBoundValue> aliasFactory)
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