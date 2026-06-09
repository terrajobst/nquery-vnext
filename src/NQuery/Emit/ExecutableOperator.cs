#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    // A node in the executable plan: the reusable counterpart of a runtime Iterator
    // (the IEnumerable to the iterator's IEnumerator). CreateIterator() produces a
    // fresh stateful iterator for one execution. Each node carries the value slots
    // it outputs so a parent can map slots to row-buffer positions.
    internal abstract class ExecutableOperator
    {
        protected ExecutableOperator(ImmutableArray<ValueSlot> outputValueSlots)
        {
            OutputValueSlots = outputValueSlots;
        }

        public ImmutableArray<ValueSlot> OutputValueSlots { get; }

        public abstract Iterator CreateIterator();

        protected static RowBufferAllocation Allocate(ExecutableOperator input, Iterator inputIterator)
        {
            return new RowBufferAllocation(null, inputIterator.RowBuffer, input.OutputValueSlots);
        }
    }
}
