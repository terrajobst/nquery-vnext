using System.Collections;

namespace NQuery.Algebra;

// The logical counterpart of BoundComparedValue: the value slot to group/sort/dedupe by, plus
// its comparer. The binder's BoundComparedValue carries an IBoundValue; the algebrizer resolves
// it to the slot here.
internal sealed class LogicalComparedValue
{
    public LogicalComparedValue(ValueSlot valueSlot, IComparer comparer)
    {
        ValueSlot = valueSlot;
        Comparer = comparer;
    }

    public ValueSlot ValueSlot { get; }

    public IComparer Comparer { get; }
}
