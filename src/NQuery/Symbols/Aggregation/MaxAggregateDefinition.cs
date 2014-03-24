using System;

namespace NQuery.Symbols.Aggregation
{
    public sealed class MaxAggregateDefinition : MinMaxAggregateDefinition
    {
        public MaxAggregateDefinition()
            : base(false)
        {
        }
    }
}