#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    // Placeholder: a surviving correlated apply (dependent nested loops). Not wired.
    internal sealed class ExecutableApply : ExecutableOperator
    {
        public ExecutableApply(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator left, ExecutableOperator right)
            : base(outputValueSlots)
        {
            Left = left;
            Right = right;
        }

        public ExecutableOperator Left { get; }

        public ExecutableOperator Right { get; }

        public override Iterator CreateIterator()
        {
            throw new NotSupportedException("Emitting an apply iterator is not yet supported.");
        }
    }
}
