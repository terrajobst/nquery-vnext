using NQuery.Binding;

namespace NQuery.Symbols;

internal sealed class QueryColumnInstanceSymbol : ColumnInstanceSymbol
{
    private readonly IBoundValue _boundValue;

    // A query column always exposes an existing value (a table column or a computed value),
    // so it aliases that value's identity rather than introducing one.
    internal QueryColumnInstanceSymbol(string name, IBoundValue boundValue)
        : base(name)
    {
        _boundValue = boundValue;
    }

    public override SymbolKind Kind
    {
        get { return SymbolKind.ColumnInstance; }
    }

    public override ColumnInstanceKind ColumnInstanceKind
    {
        get { return ColumnInstanceKind.QueryColumn; }
    }

    internal override IBoundValue BoundValue => _boundValue;

    public override Type Type => BoundValue.Type;
}
