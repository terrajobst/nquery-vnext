#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    // Placeholder: grouping + aggregation (stream/hash). Not wired.
    internal sealed class ExecutableAggregate : ExecutableOperator
    {
        public ExecutableAggregate(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator input)
            : base(outputValueSlots)
        {
            Input = input;
        }

        public ExecutableOperator Input { get; }

        public override Iterator CreateIterator(RowBuffer? outer)
        {
            throw new NotSupportedException("Emitting an aggregate iterator is not yet supported.");
        }
    }
}
