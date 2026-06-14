#nullable enable

using System.Collections.Immutable;

namespace NQuery.Algebra;

// The logical counterpart of BoundUnifiedValue: the output slot of a set operation and the
// per-input slots it unifies. The binder's BoundUnifiedValue carries IBoundValues; the
// algebrizer resolves them to slots here.
internal sealed class LogicalUnifiedValue
{
    public LogicalUnifiedValue(ValueSlot valueSlot, IEnumerable<ValueSlot> inputValueSlots)
    {
        ValueSlot = valueSlot;
        InputValueSlots = inputValueSlots.ToImmutableArray();
    }

    public ValueSlot ValueSlot { get; }

    public ImmutableArray<ValueSlot> InputValueSlots { get; }
}
