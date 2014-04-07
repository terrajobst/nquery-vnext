using System;

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
            get { return _isMin ? "MIN" : "MAX"; }
        }

        public override IAggregatable CreateAggregatable(Type argumentType)
        {
            return typeof(IComparable).IsAssignableFrom(argumentType)
                ? new MinMaxAggregatable(argumentType, _isMin)
                : null;
        }

        private sealed class MinMaxAggregatable : IAggregatable
        {
            private readonly Type _argumentType;
            private readonly bool _isMin;

            public MinMaxAggregatable(Type argumentType, bool isMin)
            {
                _argumentType = argumentType;
                _isMin = isMin;
            }

            public Type ReturnType
            {
                get { return _argumentType; }
            }

            public IAggregator CreateAggregator()
            {
                return new MinMaxAggregator(_isMin);
            }
        }

        private sealed class MinMaxAggregator : IAggregator
        {
            private readonly bool _isMin;

            private IComparable _currentMaxValue;

            public MinMaxAggregator(bool isMin)
            {
                _isMin = isMin;
            }

            public void Initialize()
            {
                _currentMaxValue = null;
            }

            public void Accumulate(object value)
            {
                var comparable = value as IComparable;
                if (comparable == null)
                    return;

                if (_currentMaxValue == null)
                {
                    _currentMaxValue = comparable;
                }
                else
                {
                    var result = _currentMaxValue.CompareTo(comparable);

                    if (_isMin)
                    {
                        if (result > 0)
                            _currentMaxValue = comparable;
                    }
                    else
                    {
                        if (result < 0)
                            _currentMaxValue = comparable;
                    }
                }
            }

            public object GetResult()
            {
                return _currentMaxValue;
            }
        }
    }
}