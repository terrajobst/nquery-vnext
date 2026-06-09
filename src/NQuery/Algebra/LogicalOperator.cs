#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Binding;

namespace NQuery.Algebra
{
    // Base of the logical relational algebra produced by the Algebrizer
    // (Bound -> Logical). Operators are immutable and flow data as value slots.
    //
    // Defined and output value slots are materialized lazily and cached: defined
    // values as a FrozenSet (used for membership tests -- "does this subtree
    // reference/contain that slot"), output values as an ImmutableArray (order is
    // meaningful -- it is the column order of the result). Caching is safe because
    // operators are immutable; the lazy initialization is interlocked so concurrent
    // reads are sound, and the concrete return types avoid boxing.
    internal abstract class LogicalOperator
    {
        private FrozenSet<ValueSlot>? _definedValueSlots;
        private ImmutableArray<ValueSlot> _outputValueSlots;

        public abstract LogicalOperatorKind Kind { get; }

        public FrozenSet<ValueSlot> DefinedValueSlots
        {
            get
            {
                var slots = _definedValueSlots;
                if (slots is null)
                {
                    slots = ComputeDefinedValueSlots();
                    slots = Interlocked.CompareExchange(ref _definedValueSlots, slots, null) ?? slots;
                }

                return slots;
            }
        }

        public ImmutableArray<ValueSlot> OutputValueSlots
        {
            get
            {
                if (_outputValueSlots.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _outputValueSlots, ComputeOutputValueSlots());

                return _outputValueSlots;
            }
        }

        protected abstract FrozenSet<ValueSlot> ComputeDefinedValueSlots();

        protected abstract ImmutableArray<ValueSlot> ComputeOutputValueSlots();
    }
}
