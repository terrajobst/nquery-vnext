using System.Globalization;
using System.Text;

namespace NQuery.Symbols.Aggregation
{
    public sealed class ConcatAggregateDefinition : AggregateDefinition
    {
        public override string Name
        {
            get { return @"CONCAT"; }
        }

        public override IAggregatable CreateAggregatable(Type argumentType)
        {
            return new ConcatAggregatable();
        }

        private sealed class ConcatAggregatable : IAggregatable
        {
            public IAggregator CreateAggregator()
            {
                return new ConcatAggregator();
            }

            public Type ReturnType
            {
                get { return typeof(string); }
            }
        }

        private sealed class ConcatAggregator : IAggregator
        {
            private readonly SortedSet<string> _valueList = new();

            public void Initialize()
            {
                _valueList.Clear();
            }

            public void Accumulate(object value)
            {
                if (value is null)
                    return;

                var strValue = Convert.ToString(value, CultureInfo.InvariantCulture).Trim();

                if (_valueList.Contains(strValue))
                    return;

                _valueList.Add(strValue);
            }

            public object GetResult()
            {
                var sb = new StringBuilder(_valueList.Count * 8);

                foreach (var value in _valueList)
                {
                    if (sb.Length > 0)
                        sb.Append(@", ");

                    sb.Append(value);
                }

                return sb.ToString();
            }
        }
    }
}