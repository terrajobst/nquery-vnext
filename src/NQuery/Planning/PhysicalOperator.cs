#nullable enable

using System.Collections.Frozen;
using System.Collections.Immutable;

using NQuery.Algebra;

namespace NQuery.Planning;

// Base of the physical operator tree produced by the Planner (Logical ->
// Physical). Same slot-flow as the logical layer -- planning chooses algorithms,
// it does not change which slots flow -- so defined/output value slots are
// materialized lazily and cached exactly as on LogicalOperator.
internal abstract class PhysicalOperator
{
    public abstract PhysicalOperatorKind Kind { get; }

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
