using NQuery.Metadata;

namespace NQuery.CodeAnalysis.Symbols;

public sealed class VariableSymbol : Symbol
{
    internal VariableSymbol(VariableDefinition definition)
        : base(GetName(definition))
    {
        Definition = definition;
    }

    private static string GetName(VariableDefinition definition)
    {
        ThrowIfNull(definition);

        return definition.Name;
    }

    public VariableDefinition Definition { get; }

    public override SymbolKind Kind
    {
        get { return SymbolKind.Variable; }
    }

    public override Type Type
    {
        get { return Definition.Type; }
    }

    public object? Value
    {
        get { return Definition.Value; }
        set { Definition.Value = value; }
    }
}
