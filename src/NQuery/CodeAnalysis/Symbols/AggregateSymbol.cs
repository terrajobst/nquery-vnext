using NQuery.Metadata;

namespace NQuery.CodeAnalysis.Symbols;

public sealed class AggregateSymbol : Symbol
{
    internal AggregateSymbol(AggregateDefinition definition)
        : base(definition.Name)
    {
        Definition = definition;
    }

    public AggregateDefinition Definition { get; }

    public override SymbolKind Kind
    {
        get { return SymbolKind.Aggregate; }
    }
}
