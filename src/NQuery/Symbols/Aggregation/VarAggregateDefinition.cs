using System;

namespace NQuery.Symbols.Aggregation
{
    public sealed class VarAggregateDefinition : VarAndStdDevAggregateDefinition
    {
        public VarAggregateDefinition()
            : base(true)
        {
        }
    }
}