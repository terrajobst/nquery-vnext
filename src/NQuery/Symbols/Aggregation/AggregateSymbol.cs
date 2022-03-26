namespace NQuery.Symbols.Aggregation
{
    public abstract class AggregateSymbol : Symbol
    {
        protected AggregateSymbol(string name)
            : base(name)
        {
        }

        public override SymbolKind Kind
        {
            get { return SymbolKind.Aggregate; }
        }

        public override Type Type
        {
            get { return TypeFacts.Missing; }
        }

        public abstract IAggregatable CreateAggregatable(Type argumentType);
    }
}