using NQuery.Binding;

namespace NQuery.Symbols;

internal sealed class TableColumnInstanceSymbol : ColumnInstanceSymbol, IBoundValue
{
    private readonly IBoundValue? _aliased;

    // Real table: the column is its own value identity (its slot is minted by the algebrizer
    // at the table scan).
    internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column)
        : base(column.Name)
    {
        TableInstance = tableInstance;
        Column = column;
    }

    // Derived table: the column aliases the inner query's value, so it resolves to that value
    // rather than introducing one of its own.
    internal TableColumnInstanceSymbol(TableInstanceSymbol tableInstance, ColumnSymbol column, IBoundValue aliased)
        : base(column.Name)
    {
        TableInstance = tableInstance;
        Column = column;
        _aliased = aliased;
    }

    public override SymbolKind Kind
    {
        get { return SymbolKind.ColumnInstance; }
    }

    public override ColumnInstanceKind ColumnInstanceKind
    {
        get { return ColumnInstanceKind.TableColumn; }
    }

    internal override IBoundValue BoundValue => _aliased ?? this;

    // A real table column is its own value identity, so its type cannot come from BoundValue
    // (that would be self-referential); it is the underlying column's type.
    public override Type Type => Column.Type;

    public override TableInstanceSymbol TableInstance { get; }

    public override ColumnSymbol Column { get; }
}
