using NQuery.Binding;

namespace NQuery.Symbols;

public abstract class ColumnInstanceSymbol : Symbol
{
    private protected ColumnInstanceSymbol(string name)
        : base(name)
    {
    }

    public abstract ColumnInstanceKind ColumnInstanceKind { get; }

    public virtual TableInstanceSymbol? TableInstance
    {
        get { return null; }
    }

    public virtual ColumnSymbol? Column
    {
        get { return null; }
    }

    // The value identity this column resolves to. The algebrizer maps it to a slot; the
    // symbol carries no slot of its own.
    internal abstract IBoundValue BoundValue { get; }
}
