using System;

namespace NQuery.Symbols.Aggregation
{
    public sealed class AggregateSymbol : Symbol
    {
        public AggregateSymbol(AggregateDefinition definition)
            : base(definition.Name)
        {
            Definition = definition;
        }

        public AggregateDefinition Definition { get; }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Aggregate; }
        }

        public override Type Type
        {
            get { return TypeFacts.Missing; }
        }
    }
}