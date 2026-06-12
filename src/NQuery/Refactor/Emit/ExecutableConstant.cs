#nullable enable

using System.Collections.Immutable;

using NQuery.Refactor.Binding;
using NQuery.Refactor.Iterators;

namespace NQuery.Refactor.Emit
{
    internal sealed class ExecutableConstant : ExecutableOperator
    {
        public ExecutableConstant(ImmutableArray<ValueSlot> outputValueSlots)
            : base(outputValueSlots)
        {
        }

        public override Iterator CreateIterator(RowBuffer? outer) => new ConstantIterator();
    }
}
