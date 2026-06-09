#nullable enable

using System.Collections.Immutable;

using NQuery.Binding;
using NQuery.EmittedIterators;

namespace NQuery.Emit
{
    // Placeholder: UNION [ALL]. Not wired.
    internal sealed class ExecutableConcatenation : ExecutableOperator
    {
        public ExecutableConcatenation(ImmutableArray<ValueSlot> outputValueSlots, ImmutableArray<ExecutableOperator> inputs)
            : base(outputValueSlots)
        {
            Inputs = inputs;
        }

        public ImmutableArray<ExecutableOperator> Inputs { get; }

        public override Iterator CreateIterator()
        {
            throw new NotSupportedException("Emitting a concatenation iterator is not yet supported.");
        }
    }
}
