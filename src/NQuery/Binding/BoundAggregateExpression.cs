using System;

using NQuery.Symbols.Aggregation;

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
                return Aggregatable == null
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

        public BoundAggregateExpression Update(AggregateSymbol aggregate, IAggregatable aggregatable, BoundExpression argument)
        {
            if (aggregate == Symbol && aggregatable == Aggregatable && argument == Argument)
                return this;

            return new BoundAggregateExpression(aggregate, aggregatable, argument);
        }

        public override string ToString()
        {
            return $"{Symbol.Name}({Argument})";
        }
    }
}