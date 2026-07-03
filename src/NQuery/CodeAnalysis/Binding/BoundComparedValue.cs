using System.Collections;

namespace NQuery.CodeAnalysis.Binding;

internal sealed class BoundComparedValue
{
    public BoundComparedValue(IBoundValue value, IComparer comparer)
    {
        ThrowIfNull(value);
        ThrowIfNull(comparer);

        Value = value;
        Comparer = comparer;
    }

    public IBoundValue Value { get; }

    public IComparer Comparer { get; }
}
