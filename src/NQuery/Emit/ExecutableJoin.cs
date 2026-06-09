#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    // Placeholder: the node exists so the executable tree covers joins, but emitting
    // a runtime iterator (nested-loops variants / hash match, with the correlated
    // row-buffer threading) is not wired yet.
    internal sealed class ExecutableJoin : ExecutableOperator
    {
        public ExecutableJoin(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator left, ExecutableOperator right)
            : base(outputValueSlots)
        {
            Left = left;
            Right = right;
        }

        public ExecutableOperator Left { get; }

        public ExecutableOperator Right { get; }

        public override Iterator CreateIterator()
        {
            throw new NotSupportedException("Emitting a join iterator is not yet supported.");
        }
    }
}
