using System;

using NQuery.Binding;
using NQuery.Symbols;

namespace NQuery.Algebra
{
    internal sealed class AlgebraAggregateDefinition
    {
        private readonly ValueSlot _output;
        private readonly AggregateSymbol _aggregate;
        private readonly AlgebraExpression _argument;

        public AlgebraAggregateDefinition(ValueSlot output, AggregateSymbol aggregate, AlgebraExpression argument)
        {
            _output = output;
            _aggregate = aggregate;
            _argument = argument;
        }

        public ValueSlot Output
        {
            get { return _output; }
        }

        public AggregateSymbol Aggregate
        {
            get { return _aggregate; }
        }

        public AlgebraExpression Argument
        {
            get { return _argument; }
        }
    }
}