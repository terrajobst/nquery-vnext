using System.Collections.Immutable;

using NQuery.Algebra;
using NQuery.Iterators;

namespace NQuery.Emit;

internal sealed class ExecutableConstant : ExecutableOperator
{
    public ExecutableConstant(ImmutableArray<ValueSlot> outputValueSlots)
        : base(outputValueSlots)
    {
    }

    public override Iterator CreateIterator(RowBuffer? outer) => new ConstantIterator();
}
