#nullable enable

using System.Collections.Immutable;

using NQuery.Refactor.Binding;
using NQuery.Refactor.Iterators;
using NQuery.Symbols;

namespace NQuery.Refactor.Emit
{
    internal sealed class ExecutableTableScan : ExecutableOperator
    {
        private readonly TableDefinition _definition;
        private readonly ImmutableArray<Func<object, object>> _columnAccessors;

        public ExecutableTableScan(ImmutableArray<ValueSlot> outputValueSlots, TableDefinition definition, ImmutableArray<Func<object, object>> columnAccessors)
            : base(outputValueSlots)
        {
            _definition = definition;
            _columnAccessors = columnAccessors;
        }

        public override Iterator CreateIterator(RowBuffer? outer) => new TableIterator(_definition, _columnAccessors);
    }
}
