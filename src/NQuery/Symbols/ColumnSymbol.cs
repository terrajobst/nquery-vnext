using NQuery.Metadata;

namespace NQuery.Symbols;

public sealed class ColumnSymbol : Symbol
{
    internal ColumnSymbol(string name, Type type)
        : base(name)
    {
        ThrowIfNull(type);

        Type = type;
    }

    internal ColumnSymbol(ColumnDefinition definition)
        : this(definition.Name, definition.DataType)
    {
        Definition = definition;
    }

    public override SymbolKind Kind
    {
        get { return SymbolKind.Column; }
    }

    public override Type Type { get; }

    public ColumnDefinition? Definition { get; }
}
