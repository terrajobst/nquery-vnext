using System.Collections;

namespace NQuery.CodeAnalysis.Iterators;

// One sort key, decoded once per row into a typed array and compared from there. This is the
// "decode once, typed" idea: the comparison is the dominant cost of a sort (O(n log n)), so
// re-decoding a column out of the spool on every comparison -- as a per-comparison reader
// does -- is what makes the columnar sort slow. Pulling each key into its own Nullable<T>[]
// up front (one O(n) pass) turns every comparison into a typed array load plus a compare, no
// decode and no boxing. NULLs sort first regardless of direction; only the value comparison
// is negated for DESC, matching the engine's order semantics.
internal abstract class SortKey
{
    // Materializes this key for every spooled row into its typed backing array.
    public abstract void Decode(SpooledRowStore store, int count);

    // Orders two rows on this key alone; 0 means the rows tie on it.
    public abstract int Compare(int x, int y);

    public static SortKey Create(RowBufferColumn column, Type type, IComparer comparer)
    {
        // DESC is carried as a NegatedComparer wrapper; recover the direction and the
        // underlying comparer so the typed path can apply the direction itself.
        var descending = comparer is NegatedComparer negated;
        var inner = descending ? ((NegatedComparer)comparer).Inner : comparer;

        if (column.Kind == RowBufferColumnKind.Object)
            return new ObjectSortKey(column, descending, inner);

        // A registered (non-default) comparer is honored through its IComparer<T> face (LookupComparer
        // guarantees one for the column type, so the typed path never boxes); the default ordering is
        // done typed through Comparer<T>.Default.
        var custom = ReferenceEquals(inner, Comparer.Default) ? null : inner;
        var keyType = typeof(TypedSortKey<>).MakeGenericType(type);
        return (SortKey)Activator.CreateInstance(keyType, column, descending, custom)!;
    }
}

// A value-typed key (the bit-packed kinds): stored unboxed in a Nullable<T>[].
internal sealed class TypedSortKey<T> : SortKey
    where T : struct
{
    private readonly RowBufferColumn _column;
    private readonly bool _descending;
    private readonly IComparer<T>? _custom;
    private T?[] _keys = Array.Empty<T?>();

    public TypedSortKey(RowBufferColumn column, bool descending, IComparer? custom)
    {
        _column = column;
        _descending = descending;
        // LookupComparer hands back a comparer that implements IComparer<T> for this column's type,
        // so the typed comparison below avoids boxing. The default case arrives as a null custom.
        _custom = (IComparer<T>?)custom;
    }

    public override void Decode(SpooledRowStore store, int count)
    {
        _keys = count == 0 ? Array.Empty<T?>() : new T?[count];

        var cursor = store.CreateCursor();
        for (var i = 0; i < count; i++)
        {
            cursor.Row = i;
            _keys[i] = _column.Kind switch
            {
                RowBufferColumnKind.Bits32 => cursor.Read32Bit<T>(_column.Index),
                RowBufferColumnKind.Bits64 => cursor.Read64Bit<T>(_column.Index),
                RowBufferColumnKind.Bits128 => cursor.Read128Bit<T>(_column.Index),
                _ => throw ExceptionBuilder.UnexpectedValue(_column.Kind)
            };
        }
    }

    public override int Compare(int x, int y)
    {
        var xValue = _keys[x];
        var yValue = _keys[y];

        if (!xValue.HasValue)
            return yValue.HasValue ? -1 : 0;
        if (!yValue.HasValue)
            return 1;

        var a = xValue.GetValueOrDefault();
        var b = yValue.GetValueOrDefault();
        var result = _custom is null ? Comparer<T>.Default.Compare(a, b) : _custom.Compare(a, b);
        return _descending ? -result : result;
    }
}

// A reference-typed (object container) key: the values are already object, so they are kept
// as-is and compared through the bound IComparer with no extra boxing.
internal sealed class ObjectSortKey : SortKey
{
    private readonly RowBufferColumn _column;
    private readonly bool _descending;
    private readonly IComparer _comparer;
    private object?[] _keys = Array.Empty<object?>();

    public ObjectSortKey(RowBufferColumn column, bool descending, IComparer comparer)
    {
        _column = column;
        _descending = descending;
        _comparer = comparer;
    }

    public override void Decode(SpooledRowStore store, int count)
    {
        _keys = count == 0 ? Array.Empty<object?>() : new object?[count];
        for (var i = 0; i < count; i++)
            _keys[i] = store.GetObject(i, _column.Index);
    }

    public override int Compare(int x, int y)
    {
        var xValue = _keys[x];
        var yValue = _keys[y];

        if (xValue is null)
            return yValue is null ? 0 : -1;
        if (yValue is null)
            return 1;

        var result = _comparer.Compare(xValue, yValue);
        return _descending ? -result : result;
    }
}
