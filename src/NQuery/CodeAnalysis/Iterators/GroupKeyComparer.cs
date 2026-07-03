using System.Collections;

namespace NQuery.CodeAnalysis.Iterators;

// Decides whether one grouping column ties between the previous group key (held in the
// stream aggregate's output buffer) and the current input row. This is the grouping
// counterpart to SortKey: a value-typed column is read unboxed through IComparer<T>, so
// the per-row same-group test the stream aggregate runs never boxes. A NULL ties only with
// another NULL (Comparer<T>.Default's null-first ordering), matching the boxed path's
// Comparer.Default semantics.
internal abstract class GroupKeyComparer
{
    // True when the column's value is equal in the two rows (each addressed by its own
    // column, since the previous key sits in the output layout and the current value in the
    // input layout).
    public abstract bool Equal(RowBuffer x, RowBufferColumn xColumn, RowBuffer y, RowBufferColumn yColumn);

    public static GroupKeyComparer Create(Type type, RowBufferColumnKind kind, IComparer comparer)
    {
        // Direction is irrelevant to equality, so a negated (DESC) comparer is unwrapped to
        // reach its typed face; grouping itself never negates, but a shared sort key might.
        var inner = comparer is NegatedComparer negated ? negated.Inner : comparer;

        if (kind == RowBufferColumnKind.Object)
            return new ObjectGroupKeyComparer(inner);

        // A registered comparer is honored through its IComparer<T> face (LookupComparer
        // guarantees one for the column type, so the typed path never boxes); the default
        // ordering runs typed through Comparer<T>.Default, arriving here as a null custom.
        var custom = ReferenceEquals(inner, Comparer.Default) ? null : inner;
        var comparerType = typeof(TypedGroupKeyComparer<>).MakeGenericType(type);
        return (GroupKeyComparer)Activator.CreateInstance(comparerType, custom)!;
    }
}

// A value-typed grouping column: both values are read unboxed and compared through the
// bound IComparer<T> (or Comparer<T>.Default).
internal sealed class TypedGroupKeyComparer<T> : GroupKeyComparer
    where T : struct
{
    private readonly IComparer<T>? _custom;

    public TypedGroupKeyComparer(IComparer? custom)
    {
        _custom = (IComparer<T>?)custom;
    }

    public override bool Equal(RowBuffer x, RowBufferColumn xColumn, RowBuffer y, RowBufferColumn yColumn)
    {
        var xValue = x.ReadValue<T>(xColumn);
        var yValue = y.ReadValue<T>(yColumn);

        if (!xValue.HasValue)
            return !yValue.HasValue;
        if (!yValue.HasValue)
            return false;

        var a = xValue.GetValueOrDefault();
        var b = yValue.GetValueOrDefault();
        return (_custom is null ? Comparer<T>.Default.Compare(a, b) : _custom.Compare(a, b)) == 0;
    }
}

// A reference-typed (object container) grouping column: the values are already object, so
// they are compared through the bound IComparer with no extra boxing.
internal sealed class ObjectGroupKeyComparer : GroupKeyComparer
{
    private readonly IComparer _comparer;

    public ObjectGroupKeyComparer(IComparer comparer)
    {
        ThrowIfNull(comparer);

        _comparer = comparer;
    }

    public override bool Equal(RowBuffer x, RowBufferColumn xColumn, RowBuffer y, RowBufferColumn yColumn)
    {
        var xValue = x.GetObject(xColumn.Index);
        var yValue = y.GetObject(yColumn.Index);

        if (xValue is null)
            return yValue is null;
        if (yValue is null)
            return false;

        return _comparer.Compare(xValue, yValue) == 0;
    }
}
