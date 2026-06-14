using NQuery.Symbols.Aggregation;

using NQuery.Binding;

namespace NQuery.Binding
{
    internal sealed class BoundAggregateExpression : BoundExpression
    {
        public BoundAggregateExpression(AggregateSymbol aggregate, IAggregatable aggregatable, BoundExpression argument)
        {
            Symbol = aggregate;
            Aggregatable = aggregatable;
            Argument = argument;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.AggregateExpression; }
        }

        public override Type Type
        {
            get
            {
                return Aggregatable is null
                    ? TypeFacts.Unknown
                    : Aggregatable.ReturnType;
            }
        }

        public AggregateSymbol Symbol { get; }

        public AggregateSymbol Aggregate
        {
            get { return Symbol; }
        }

        public IAggregatable Aggregatable { get; }

        public BoundExpression Argument { get; }

        public override string ToString()
        {
            return $"{Symbol.Name}({Argument})";
        }
    }
}