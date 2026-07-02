using System.Runtime.CompilerServices;

namespace NQuery.CodeAnalysis.Iterators;

// The one leaf row buffer that actually owns storage: a parallel object[] and three
// width-bucketed arrays of raw bits, each value type's array carrying a packed null
// bitmask (the object container needs none -- a null reference is the null). The Write*
// methods are the counterpart of the base Read*: they maintain the bitmask and take a
// Nullable<T>, so producers never hand-roll null handling. Every other RowBuffer is a
// view; this is what Clone() and the buffering iterators materialize into.
internal sealed class ArrayRowBuffer : RowBuffer
{
    private readonly object?[] _objects;
    private readonly uint[] _bits32;
    private readonly ulong[] _bits64;
    private readonly Int128[] _bits128;
    private readonly ulong[] _null32;
    private readonly ulong[] _null64;
    private readonly ulong[] _null128;

    public ArrayRowBuffer(int objectCount, int bits32Count, int bits64Count, int bits128Count)
    {
        _objects = objectCount == 0 ? [] : new object?[objectCount];
        _bits32 = bits32Count == 0 ? [] : new uint[bits32Count];
        _bits64 = bits64Count == 0 ? [] : new ulong[bits64Count];
        _bits128 = bits128Count == 0 ? [] : new Int128[bits128Count];
        _null32 = NewNullMask(bits32Count);
        _null64 = NewNullMask(bits64Count);
        _null128 = NewNullMask(bits128Count);
    }

    public ArrayRowBuffer(RowBufferLayout layout)
        : this(layout.ObjectCount, layout.Bits32Count, layout.Bits64Count, layout.Bits128Count)
    {
    }

    public override int ObjectCount => _objects.Length;

    public override int Bits32Count => _bits32.Length;

    public override int Bits64Count => _bits64.Length;

    public override int Bits128Count => _bits128.Length;

    public override object? GetObject(int index) => _objects[index];

    public override uint GetBits32(int index) => _bits32[index];

    public override ulong GetBits64(int index) => _bits64[index];

    public override Int128 GetBits128(int index) => _bits128[index];

    public override bool IsNull32(int index) => GetNull(_null32, index);

    public override bool IsNull64(int index) => GetNull(_null64, index);

    public override bool IsNull128(int index) => GetNull(_null128, index);

    public void WriteObject<T>(int index, T value)
    {
        _objects[index] = value;
    }

    public void Write32Bit<T>(int index, T? value)
        where T : struct
    {
        if (!value.HasValue)
        {
            SetNull(_null32, index, true);
            _bits32[index] = 0;
            return;
        }

        SetNull(_null32, index, false);
        var v = value.GetValueOrDefault();
        uint raw;

        if (typeof(T) == typeof(int)) raw = (uint)Unsafe.As<T, int>(ref v);
        else if (typeof(T) == typeof(uint)) raw = Unsafe.As<T, uint>(ref v);
        else if (typeof(T) == typeof(short)) raw = (uint)(ushort)Unsafe.As<T, short>(ref v);
        else if (typeof(T) == typeof(ushort)) raw = Unsafe.As<T, ushort>(ref v);
        else if (typeof(T) == typeof(byte)) raw = Unsafe.As<T, byte>(ref v);
        else if (typeof(T) == typeof(sbyte)) raw = (uint)(byte)Unsafe.As<T, sbyte>(ref v);
        else if (typeof(T) == typeof(char)) raw = Unsafe.As<T, char>(ref v);
        else if (typeof(T) == typeof(bool)) raw = Unsafe.As<T, bool>(ref v) ? 1u : 0u;
        else if (typeof(T) == typeof(float)) { var f = Unsafe.As<T, float>(ref v); raw = Unsafe.As<float, uint>(ref f); }
        else throw new NotSupportedException($"{typeof(T)} is not a 32-bit row buffer type.");

        _bits32[index] = raw;
    }

    public void Write64Bit<T>(int index, T? value)
        where T : struct
    {
        if (!value.HasValue)
        {
            SetNull(_null64, index, true);
            _bits64[index] = 0;
            return;
        }

        SetNull(_null64, index, false);
        var v = value.GetValueOrDefault();
        ulong raw;

        if (typeof(T) == typeof(long)) raw = (ulong)Unsafe.As<T, long>(ref v);
        else if (typeof(T) == typeof(ulong)) raw = Unsafe.As<T, ulong>(ref v);
        else if (typeof(T) == typeof(double)) { var d = Unsafe.As<T, double>(ref v); raw = Unsafe.As<double, ulong>(ref d); }
        else if (typeof(T) == typeof(DateTime)) { var dt = Unsafe.As<T, DateTime>(ref v); raw = Unsafe.As<DateTime, ulong>(ref dt); }
        else if (typeof(T) == typeof(TimeSpan)) { var ts = Unsafe.As<T, TimeSpan>(ref v); raw = Unsafe.As<TimeSpan, ulong>(ref ts); }
        else throw new NotSupportedException($"{typeof(T)} is not a 64-bit row buffer type.");

        _bits64[index] = raw;
    }

    public void Write128Bit<T>(int index, T? value)
        where T : struct
    {
        if (!value.HasValue)
        {
            SetNull(_null128, index, true);
            _bits128[index] = default;
            return;
        }

        SetNull(_null128, index, false);
        var v = value.GetValueOrDefault();
        Int128 raw;

        if (typeof(T) == typeof(decimal)) { var d = Unsafe.As<T, decimal>(ref v); raw = Unsafe.As<decimal, Int128>(ref d); }
        else if (typeof(T) == typeof(Guid)) { var g = Unsafe.As<T, Guid>(ref v); raw = Unsafe.As<Guid, Int128>(ref g); }
        else throw new NotSupportedException($"{typeof(T)} is not a 128-bit row buffer type.");

        _bits128[index] = raw;
    }

    // Stores a boxed value into a column -- the write counterpart of RowBuffer.GetBoxedValue,
    // used at the object-typed boundaries (and the test harness). `type` is the column's
    // non-nullable CLR type, used to pick the decode.
    public void WriteBoxed(RowBufferColumn column, Type type, object? value)
    {
        switch (column.Kind)
        {
            case RowBufferColumnKind.Object:
                _objects[column.Index] = value;
                break;
            case RowBufferColumnKind.Bits32:
                WriteBoxed32(column.Index, type, value);
                break;
            case RowBufferColumnKind.Bits64:
                WriteBoxed64(column.Index, type, value);
                break;
            case RowBufferColumnKind.Bits128:
                WriteBoxed128(column.Index, type, value);
                break;
            default:
                throw ExceptionBuilder.UnexpectedValue(column.Kind);
        }
    }

    private void WriteBoxed32(int index, Type type, object? value)
    {
        if (type == typeof(int)) Write32Bit(index, (int?)value);
        else if (type == typeof(uint)) Write32Bit(index, (uint?)value);
        else if (type == typeof(short)) Write32Bit(index, (short?)value);
        else if (type == typeof(ushort)) Write32Bit(index, (ushort?)value);
        else if (type == typeof(byte)) Write32Bit(index, (byte?)value);
        else if (type == typeof(sbyte)) Write32Bit(index, (sbyte?)value);
        else if (type == typeof(char)) Write32Bit(index, (char?)value);
        else if (type == typeof(bool)) Write32Bit(index, (bool?)value);
        else if (type == typeof(float)) Write32Bit(index, (float?)value);
        else throw new NotSupportedException($"{type} is not a 32-bit row buffer type.");
    }

    private void WriteBoxed64(int index, Type type, object? value)
    {
        if (type == typeof(long)) Write64Bit(index, (long?)value);
        else if (type == typeof(ulong)) Write64Bit(index, (ulong?)value);
        else if (type == typeof(double)) Write64Bit(index, (double?)value);
        else if (type == typeof(DateTime)) Write64Bit(index, (DateTime?)value);
        else if (type == typeof(TimeSpan)) Write64Bit(index, (TimeSpan?)value);
        else throw new NotSupportedException($"{type} is not a 64-bit row buffer type.");
    }

    private void WriteBoxed128(int index, Type type, object? value)
    {
        if (type == typeof(decimal)) Write128Bit(index, (decimal?)value);
        else if (type == typeof(Guid)) Write128Bit(index, (Guid?)value);
        else throw new NotSupportedException($"{type} is not a 128-bit row buffer type.");
    }

    // Copies one column from another buffer into this one, raw (no boxing). The source
    // and target columns share a container, so the kind drives the copy.
    public void CopyFrom(RowBuffer source, RowBufferColumn sourceColumn, RowBufferColumn targetColumn)
    {
        switch (sourceColumn.Kind)
        {
            case RowBufferColumnKind.Object:
                _objects[targetColumn.Index] = source.GetObject(sourceColumn.Index);
                break;
            case RowBufferColumnKind.Bits32:
                SetBits32Raw(targetColumn.Index, source.GetBits32(sourceColumn.Index), source.IsNull32(sourceColumn.Index));
                break;
            case RowBufferColumnKind.Bits64:
                SetBits64Raw(targetColumn.Index, source.GetBits64(sourceColumn.Index), source.IsNull64(sourceColumn.Index));
                break;
            case RowBufferColumnKind.Bits128:
                SetBits128Raw(targetColumn.Index, source.GetBits128(sourceColumn.Index), source.IsNull128(sourceColumn.Index));
                break;
            default:
                throw ExceptionBuilder.UnexpectedValue(sourceColumn.Kind);
        }
    }

    // Raw, type-agnostic stores used by the per-container gluing copies (see CopyFrom).
    internal void SetBits32Raw(int index, uint value, bool isNull)
    {
        _bits32[index] = value;
        SetNull(_null32, index, isNull);
    }

    internal void SetBits64Raw(int index, ulong value, bool isNull)
    {
        _bits64[index] = value;
        SetNull(_null64, index, isNull);
    }

    internal void SetBits128Raw(int index, Int128 value, bool isNull)
    {
        _bits128[index] = value;
        SetNull(_null128, index, isNull);
    }

    private static ulong[] NewNullMask(int count)
    {
        return count == 0 ? [] : new ulong[(count + 63) >> 6];
    }

    private static bool GetNull(ulong[] mask, int index)
    {
        return (mask[index >> 6] & (1UL << (index & 63))) != 0;
    }

    private static void SetNull(ulong[] mask, int index, bool value)
    {
        var bit = 1UL << (index & 63);
        if (value)
            mask[index >> 6] |= bit;
        else
            mask[index >> 6] &= ~bit;
    }
}
