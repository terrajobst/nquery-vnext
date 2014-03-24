using System;

namespace NQuery.Symbols.Aggregation
{
    public sealed class AggregateSymbol : Symbol
    {
        private readonly AggregateDefinition _definition;

        public AggregateSymbol(AggregateDefinition definition)
            : base(definition.Name)
        {
            _definition = definition;
        }

        public AggregateDefinition Definition
        {
            get { return _definition; }
        }

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