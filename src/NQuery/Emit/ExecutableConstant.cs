#nullable enable

using System.Collections.Immutable;

using NQuery.AlgebraBinding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
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
