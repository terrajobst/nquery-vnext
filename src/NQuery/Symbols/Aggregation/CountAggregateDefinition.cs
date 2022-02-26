namespace NQuery.Symbols.Aggregation
{
    public sealed class CountAggregateDefinition : AggregateDefinition
    {
        public override string Name
        {
            get { return @"COUNT"; }
        }

        public override IAggregatable CreateAggregatable(Type argumentType)
        {
            return new CountAggregatable();
        }

        private sealed class CountAggregatable : IAggregatable
        {
            public IAggregator CreateAggregator()
            {
                return new CountAggregator();
            }

            public Type ReturnType
            {
                get { return typeof(int); }
            }
        }

        private sealed class CountAggregator : IAggregator
        {
            private int _count;

            public void Initialize()
            {
                _count = 0;
            }

            public void Accumulate(object value)
            {
                if (value != null)
                    _count++;
            }

            public object GetResult()
            {
                return _count;
            }
        }
    }
}