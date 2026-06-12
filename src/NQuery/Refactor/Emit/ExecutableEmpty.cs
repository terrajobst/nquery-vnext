#nullable enable

using System.Collections.Immutable;

using NQuery.Refactor.Binding;
using NQuery.Refactor.Iterators;

namespace NQuery.Refactor.Emit
{
    internal sealed class ExecutableEmpty : ExecutableOperator
    {
        public ExecutableEmpty(ImmutableArray<ValueSlot> outputValueSlots)
            : base(outputValueSlots)
        {
        }

        public override Iterator CreateIterator(RowBuffer? outer) => new EmptyIterator();
    }
}
