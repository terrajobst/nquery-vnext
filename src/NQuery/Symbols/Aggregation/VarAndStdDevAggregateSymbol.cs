using System.Globalization;

namespace NQuery.Symbols.Aggregation
{
    public abstract class VarAndStdDevAggregateSymbol : AggregateSymbol
    {
        private readonly bool _isVar;

        protected VarAndStdDevAggregateSymbol(bool isVar)
            : base(isVar ? @"VAR" : @"STDEV")
        {
            _isVar = isVar;
        }

        public override IAggregatable CreateAggregatable(Type argumentType)
        {
            if (argumentType == typeof(byte) ||
                argumentType == typeof(sbyte) ||
                argumentType == typeof(short) ||
                argumentType == typeof(ushort) ||
                argumentType == typeof(int) ||
                argumentType == typeof(uint) ||
                argumentType == typeof(long) ||
                argumentType == typeof(ulong) ||
                argumentType == typeof(decimal) ||
                argumentType == typeof(float) ||
                argumentType == typeof(double))
                return new VarAndStdDevAggregatable(_isVar);

            return null;
        }

        private sealed class VarAndStdDevAggregatable : IAggregatable
        {
            private readonly bool _isVar;

            public VarAndStdDevAggregatable(bool isVar)
            {
                _isVar = isVar;
            }

            public IAggregator CreateAggregator()
            {
                return new VarAndStdDevAggregator(_isVar);
            }

            public Type ReturnType
            {
                get
                {
                    return _isVar
                        ? typeof(decimal)
                        : typeof(double);
                }
            }
        }

        private sealed class VarAndStdDevAggregator : IAggregator
        {
            private readonly bool _isVar;

            private decimal _sum;
            private decimal _sumOfSquares;
            private int _count;

            public VarAndStdDevAggregator(bool isVar)
            {
                _isVar = isVar;
            }

            public void Initialize()
            {
                _sum = 0;
                _sumOfSquares = 0;
                _count = 0;
            }

            public void Accumulate(object value)
            {
                var valueAsDecimal = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                _sum += valueAsDecimal;
                _sumOfSquares += valueAsDecimal * valueAsDecimal;
                _count++;
            }

            public object GetResult()
            {
                if (_count < 2)
                    return null;

                var e = _sum / _count;
                var r = (_sumOfSquares - e * (2 * _sum - e * _count)) / (_count - 1);

                if (_isVar)
                    return r;

                return Math.Sqrt((double)r);
            }
        }
    }
}