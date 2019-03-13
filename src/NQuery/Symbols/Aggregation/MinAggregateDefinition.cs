#nullable enable

using System;

namespace NQuery.Symbols.Aggregation
{
    public sealed class MinAggregateDefinition : MinMaxAggregateDefinition
    {
        public MinAggregateDefinition()
            : base(true)
        {
        }
    }
}