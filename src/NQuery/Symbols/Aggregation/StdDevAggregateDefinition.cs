#nullable enable

using System;

namespace NQuery.Symbols.Aggregation
{
    public sealed class StdDevAggregateDefinition : VarAndStdDevAggregateDefinition
    {
        public StdDevAggregateDefinition()
            : base(false)
        {
        }
    }
}