using System.Collections;

namespace NQuery.Binding;

internal sealed class BoundComparedValue
{
    public BoundComparedValue(IBoundValue value, IComparer comparer)
    {
        Value = value;
        Comparer = comparer;
    }

    public IBoundValue Value { get; }

    public IComparer Comparer { get; }
}
