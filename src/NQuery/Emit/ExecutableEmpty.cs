#nullable enable

using System.Collections.Immutable;

using NQuery.AlgebraBinding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
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
