namespace NQuery.Symbols.Aggregation
{
    public abstract class MinMaxAggregateDefinition : AggregateDefinition
    {
        private readonly bool _isMin;

        protected MinMaxAggregateDefinition(bool isMin)
        {
            _isMin = isMin;
        }

        public override string Name
        {
            get { return _isMin ? @"MIN" : @"MAX"; }
        }

        public override IAggregatable CreateAggregatable(Type argumentType)
        {
            return typeof(IComparable).IsAssignableFrom(argumentType)
                ? new MinMaxAggregatable(argumentType, _isMin)
                : null;
        }

        private sealed class MinMaxAggregatable : IAggregatable
        {
            private readonly bool _isMin;

            public MinMaxAggregatable(Type argumentType, bool isMin)
            {
                ReturnType = argumentType;
                _isMin = isMin;
            }

            public Type ReturnType { get; }

            public IAggregator CreateAggregator()
            {
                return new MinMaxAggregator(_isMin);
            }
        }

        private sealed class MinMaxAggregator : IAggregator
        {
            private readonly bool _isMin;

            private IComparable _result;

            public MinMaxAggregator(bool isMin)
            {
                _isMin = isMin;
            }

            public void Initialize()
            {
                _result = null;
            }

            public void Accumulate(object value)
            {
                if (value is not IComparable comparable)
                    return;

                if (_result is null)
                {
                    _result = comparable;
                }
                else
                {
                    var comparison = _result.CompareTo(comparable);

                    if (_isMin)
                    {
                        if (comparison > 0)
                            _result = comparable;
                    }
                    else
                    {
                        if (comparison < 0)
                            _result = comparable;
                    }
                }
            }

            public object GetResult()
            {
                return _result;
            }
        }
    }
}