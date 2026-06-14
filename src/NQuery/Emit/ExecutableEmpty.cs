using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Iterators;

namespace NQuery.Emit;

internal sealed class ExecutableEmpty : ExecutableOperator
{
    public ExecutableEmpty(ImmutableArray<ValueSlot> outputValueSlots)
        : base(outputValueSlots)
    {
    }

    public override Iterator CreateIterator(RowBuffer? outer) => new EmptyIterator();
}
