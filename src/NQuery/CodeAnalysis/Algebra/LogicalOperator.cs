using System.Collections.Frozen;
using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Algebra;

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
    public abstract LogicalOperatorKind Kind { get; }

    public FrozenSet<ValueSlot> DefinedValueSlots
    {
        get
        {
            if (field is null)
                Interlocked.CompareExchange(ref field, ComputeDefinedValueSlots(), null);

            return field;
        }
    }

    public ImmutableArray<ValueSlot> OutputValueSlots
    {
        get
        {
            if (field.IsDefault)
                ImmutableInterlocked.InterlockedInitialize(ref field, ComputeOutputValueSlots());

            return field;
        }
    }

    protected abstract FrozenSet<ValueSlot> ComputeDefinedValueSlots();

    protected abstract ImmutableArray<ValueSlot> ComputeOutputValueSlots();
}
