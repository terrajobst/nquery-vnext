using System.Collections;

namespace NQuery.CodeAnalysis.Iterators;

// One tie-breaking column of a TOP ... WITH TIES. The iterator emits the limit-th row, then
// keeps emitting rows that tie with it on the ORDER BY columns. Deciding a tie used to box
// both the captured value and each candidate row's value; instead each column captures the
// last emitted value into a typed field and compares the next row unboxed (see SortKey for
// the same decode-typed idea). Direction is irrelevant to a tie, so a DESC (negated)
// comparer is unwrapped; a NULL ties only with another NULL.
internal abstract class TieColumn
{
    // Records this column's value from the row currently in the buffer (the just-emitted row).
    public abstract void Capture();

    // True when the row currently in the buffer ties with the captured value on this column.
    public abstract bool MatchesCaptured();

    public static TieColumn Create(RowBufferEntry entry, IComparer comparer)
    {
        var inner = comparer is NegatedComparer negated ? negated.Inner : comparer;

        if (entry.Column.Kind == RowBufferColumnKind.Object)
            return new ObjectTieColumn(entry, inner);

        var custom = ReferenceEquals(inner, Comparer.Default) ? null : inner;
        var columnType = typeof(TypedTieColumn<>).MakeGenericType(entry.Type);
        return (TieColumn)Activator.CreateInstance(columnType, entry, custom)!;
    }
}

// A value-typed tie column: the captured value is held unboxed in a Nullable<T> field and
// compared through the bound IComparer<T> (or Comparer<T>.Default).
internal sealed class TypedTieColumn<T> : TieColumn
    where T : struct
{
    private readonly RowBufferEntry _entry;
    private readonly IComparer<T>? _custom;
    private T? _captured;

    public TypedTieColumn(RowBufferEntry entry, IComparer? custom)
    {
        _entry = entry;
        _custom = (IComparer<T>?)custom;
    }

    public override void Capture()
    {
        _captured = _entry.RowBuffer.ReadValue<T>(_entry.Column);
    }

    public override bool MatchesCaptured()
    {
        var current = _entry.RowBuffer.ReadValue<T>(_entry.Column);

        if (!_captured.HasValue)
            return !current.HasValue;
        if (!current.HasValue)
            return false;

        var a = _captured.GetValueOrDefault();
        var b = current.GetValueOrDefault();
        return (_custom is null ? Comparer<T>.Default.Compare(a, b) : _custom.Compare(a, b)) == 0;
    }
}

// A reference-typed (object container) tie column: the value is already object, so it is
// kept as-is and compared through the bound IComparer with no extra boxing.
internal sealed class ObjectTieColumn : TieColumn
{
    private readonly RowBufferEntry _entry;
    private readonly IComparer _comparer;
    private object? _captured;

    public ObjectTieColumn(RowBufferEntry entry, IComparer comparer)
    {
        _entry = entry;
        _comparer = comparer;
    }

    public override void Capture()
    {
        _captured = _entry.RowBuffer.GetObject(_entry.Column.Index);
    }

    public override bool MatchesCaptured()
    {
        var current = _entry.RowBuffer.GetObject(_entry.Column.Index);

        if (_captured is null)
            return current is null;
        if (current is null)
            return false;

        return _comparer.Compare(_captured, current) == 0;
    }
}
