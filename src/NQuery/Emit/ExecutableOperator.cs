using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Iterators;

namespace NQuery.Emit;

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

    // outer is the row buffer of the enclosing Apply's left side (its correlated
    // outer references), or null at the top level. Operators thread it to their
    // inputs; correlated filters/computes read from it.
    public abstract Iterator CreateIterator(RowBuffer? outer);

    protected static RowBufferAllocation Allocate(ExecutableOperator input, Iterator inputIterator)
    {
        return new RowBufferAllocation(null, inputIterator.RowBuffer, input.OutputValueSlots);
    }
}
