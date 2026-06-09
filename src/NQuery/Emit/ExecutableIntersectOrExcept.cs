#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    // Placeholder: INTERSECT / EXCEPT. Not wired.
    internal sealed class ExecutableIntersectOrExcept : ExecutableOperator
    {
        public ExecutableIntersectOrExcept(ImmutableArray<ValueSlot> outputValueSlots, ExecutableOperator left, ExecutableOperator right)
            : base(outputValueSlots)
        {
            Left = left;
            Right = right;
        }

        public ExecutableOperator Left { get; }

        public ExecutableOperator Right { get; }

        public override Iterator CreateIterator()
        {
            throw new NotSupportedException("Emitting an intersect/except iterator is not yet supported.");
        }
    }
}
