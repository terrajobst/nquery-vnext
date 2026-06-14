using System.Linq.Expressions;

using NQuery.Metadata;

namespace NQuery.Symbols;

public sealed class PropertySymbol : Symbol
{
    public PropertySymbol(PropertyDefinition definition)
        : base(GetName(definition))
    {
        Definition = definition;
    }

    private static string GetName(PropertyDefinition definition)
    {
        ThrowIfNull(definition);

        return definition.Name;
    }

    public PropertyDefinition Definition { get; }

    public override SymbolKind Kind
    {
        get { return SymbolKind.Property; }
    }

    public override Type Type
    {
        get { return Definition.Type; }
    }

    internal Expression CreateInvocation(Expression instance)
    {
        return Definition.CreateInvocation(instance);
    }
}
