using System.Collections.Immutable;

using NQuery.CodeAnalysis.Algebra;
using NQuery.CodeAnalysis.Iterators;

namespace NQuery.CodeAnalysis.Emit;

internal sealed class ExecutableConstant : ExecutableOperator
{
    public ExecutableConstant(ImmutableArray<ValueSlot> outputValueSlots)
        : base(outputValueSlots)
    {
    }

    public override Iterator CreateIterator(RecursiveWorkTableRegistry workTables, RowBuffer? outer) => new ConstantIterator();
}
