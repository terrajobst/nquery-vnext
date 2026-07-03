using System.Collections;

namespace NQuery.CodeAnalysis.Iterators;

internal sealed class NegatedComparer : IComparer
{
    private readonly IComparer _comparer;

    public NegatedComparer(IComparer comparer)
    {
        ThrowIfNull(comparer);

        _comparer = comparer;
    }

    // The wrapped (ascending) comparer. The row-comparer compiler recovers it to emit a
    // typed comparison with the direction applied as a negation, rather than calling
    // through this object-based wrapper.
    public IComparer Inner => _comparer;

    public int Compare(object? x, object? y)
    {
        return -_comparer.Compare(x, y);
    }
}
