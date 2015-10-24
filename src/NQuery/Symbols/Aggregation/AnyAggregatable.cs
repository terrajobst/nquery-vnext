using System;

namespace NQuery.Symbols.Aggregation
{
    public sealed class AnyAggregateDefinition : AggregateDefinition
    {
        public override string Name
        {
            get { return "ANY"; }
        }

        public override IAggregatable CreateAggregatable(Type argumentType)
        {
            return new AnyAggregatable(argumentType);
        }

        private sealed class AnyAggregatable : IAggregatable
        {
            public AnyAggregatable(Type returnType)
            {
                ReturnType = returnType;
            }

            public IAggregator CreateAggregator()
            {
                return new AnyAggregator();
            }

            public Type ReturnType { get; }
        }

        private sealed class AnyAggregator : IAggregator
        {
            private object _value;

            public void Initialize()
            {
                _value = null;
            }

            public void Accumulate(object value)
            {
                _value = value;
            }

            public object GetResult()
            {
                return _value;
            }
        }
    }
}