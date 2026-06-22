using System.Collections;

namespace NQuery;

// The catalog stores comparers as the non-generic IComparer, but the iterator pipeline wants to
// compare values through IComparer<T> so a typed (unboxed) sort key never has to box. Because the
// comparer for a type can be registered against a base type and recovered for a derived one, the
// "also implements IComparer<T>" guarantee is established per requested type at lookup time (see
// GlobalBinder.LookupComparer) rather than at registration. These adapters bridge whichever
// interface a caller supplied to the one it is missing.
internal static class CatalogComparer
{
    // Returns a comparer that implements IComparer<T> for the requested type, wrapping a non-generic
    // comparer only when it does not already satisfy it (e.g. Comparer<T> and a registered
    // IComparer<T> are returned as-is, with no extra boxing).
    public static IComparer EnsureTyped(Type type, IComparer comparer)
    {
        var typedComparerType = typeof(IComparer<>).MakeGenericType(type);
        if (typedComparerType.IsInstanceOfType(comparer))
            return comparer;

        var adapterType = typeof(BoxingComparer<>).MakeGenericType(type);
        return (IComparer)Activator.CreateInstance(adapterType, comparer)!;
    }
}

// Adapts a non-generic IComparer to IComparer<T>. The typed path boxes through the inner comparer;
// callers that want to avoid that allocation can register an IComparer<T> directly.
internal sealed class BoxingComparer<T> : IComparer, IComparer<T>
{
    private readonly IComparer _inner;

    public BoxingComparer(IComparer inner)
    {
        _inner = inner;
    }

    public int Compare(T? x, T? y)
    {
        return _inner.Compare(x, y);
    }

    public int Compare(object? x, object? y)
    {
        return _inner.Compare(x, y);
    }
}

// Adapts an IComparer<T> to the non-generic IComparer used by the parts of the pipeline that still
// compare boxed values (union, group, top). Only needed when a registered IComparer<T> does not
// itself implement IComparer; the common case (Comparer<T> and its subclasses) implements both and
// is stored as-is.
internal sealed class UnboxingComparer<T> : IComparer, IComparer<T>
{
    private readonly IComparer<T> _inner;

    public UnboxingComparer(IComparer<T> inner)
    {
        _inner = inner;
    }

    public int Compare(T? x, T? y)
    {
        // IComparer<T>.Compare is annotated non-nullable on this target framework, but the inner
        // comparer is expected to mirror Comparer<T> and handle nulls, so forwarding them is safe.
        return _inner.Compare(x!, y!);
    }

    public int Compare(object? x, object? y)
    {
        // Mirror Comparer<T>'s non-generic behavior: nulls sort first and never reach the typed
        // comparer, so a value-typed T is only ever unboxed from a non-null reference.
        if (ReferenceEquals(x, y))
            return 0;
        if (x is null)
            return -1;
        if (y is null)
            return 1;

        return _inner.Compare((T)x, (T)y);
    }
}
