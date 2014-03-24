using System;

using NQuery.Symbols.Aggregation;

namespace NQuery.Binding
{
    internal sealed class BoundAggregateExpression : BoundExpression
    {
        private readonly AggregateSymbol _aggregate;
        private readonly IAggregatable _aggregatable;
        private readonly BoundExpression _argument;

        public BoundAggregateExpression(AggregateSymbol aggregate, IAggregatable aggregatable, BoundExpression argument)
        {
            _aggregate = aggregate;
            _aggregatable = aggregatable;
            _argument = argument;
        }

        public override BoundNodeKind Kind
        {
            get { return BoundNodeKind.AggregateExpression; }
        }

        public override Type Type
        {
            get
            {
                return _aggregatable == null
                    ? TypeFacts.Unknown
                    : _aggregatable.ReturnType;
            }
        }

        public AggregateSymbol Symbol
        {
            get { return _aggregate; }
        }

        public AggregateSymbol Aggregate
        {
            get { return _aggregate; }
        }

        public IAggregatable Aggregatable
        {
            get { return _aggregatable; }
        }

        public BoundExpression Argument
        {
            get { return _argument; }
        }

        public BoundAggregateExpression Update(AggregateSymbol aggregate, IAggregatable aggregatable, BoundExpression argument)
        {
            if (aggregate == _aggregate && aggregatable == _aggregatable && argument == _argument)
                return this;

            return new BoundAggregateExpression(aggregate, aggregatable, argument);
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", _aggregate.Name, _argument);
        }
    }
}