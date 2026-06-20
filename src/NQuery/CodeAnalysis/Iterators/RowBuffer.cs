using System.Runtime.CompilerServices;

namespace NQuery.CodeAnalysis.Iterators;

// A row exposed as four parallel containers (see RowBufferColumnKind). The typed
// Read* methods return the column's value already lifted to its nullable shape --
// Nullable<T> for value types, a possibly-null reference for the object container --
// so SQL NULL flows uniformly without the engine boxing every primitive cell.
//
// Compound buffers (Combined, Projected, ...) glue their inputs per container, so a
// container index stays container-relative across the join boundary. Raw, untyped
// access (GetObject/GetBits*/IsNull*) is the primitive every typed read, every
// boxed boundary read, and Clone() is built on.
internal abstract class RowBuffer
{
    public abstract int ObjectCount { get; }

    public abstract int Bits32Count { get; }

    public abstract int Bits64Count { get; }

    public abstract int Bits128Count { get; }

    public abstract object? GetObject(int index);

    public abstract uint GetBits32(int index);

    public abstract ulong GetBits64(int index);

    public abstract Int128 GetBits128(int index);

    public abstract bool IsNull32(int index);

    public abstract bool IsNull64(int index);

    public abstract bool IsNull128(int index);

    // The object container already holds a reference (or a boxed value); the caller
    // picks T as the column's nullable type, so a reference cast or an unbox-to-Nullable
    // does the right thing for a null cell.
    public T ReadObject<T>(int index)
    {
        return (T)GetObject(index)!;
    }

    public T? Read32Bit<T>(int index)
        where T : struct
    {
        if (IsNull32(index))
            return null;

        var raw = GetBits32(index);

        if (typeof(T) == typeof(int)) { var v = (int)raw; return Unsafe.As<int, T>(ref v); }
        if (typeof(T) == typeof(uint)) { var v = raw; return Unsafe.As<uint, T>(ref v); }
        if (typeof(T) == typeof(short)) { var v = (short)raw; return Unsafe.As<short, T>(ref v); }
        if (typeof(T) == typeof(ushort)) { var v = (ushort)raw; return Unsafe.As<ushort, T>(ref v); }
        if (typeof(T) == typeof(byte)) { var v = (byte)raw; return Unsafe.As<byte, T>(ref v); }
        if (typeof(T) == typeof(sbyte)) { var v = (sbyte)raw; return Unsafe.As<sbyte, T>(ref v); }
        if (typeof(T) == typeof(char)) { var v = (char)raw; return Unsafe.As<char, T>(ref v); }
        if (typeof(T) == typeof(bool)) { var v = raw != 0; return Unsafe.As<bool, T>(ref v); }
        if (typeof(T) == typeof(float)) { var v = Unsafe.As<uint, float>(ref raw); return Unsafe.As<float, T>(ref v); }

        throw new NotSupportedException($"{typeof(T)} is not a 32-bit row buffer type.");
    }

    public T? Read64Bit<T>(int index)
        where T : struct
    {
        if (IsNull64(index))
            return null;

        var raw = GetBits64(index);

        if (typeof(T) == typeof(long)) { var v = (long)raw; return Unsafe.As<long, T>(ref v); }
        if (typeof(T) == typeof(ulong)) { var v = raw; return Unsafe.As<ulong, T>(ref v); }
        if (typeof(T) == typeof(double)) { var v = Unsafe.As<ulong, double>(ref raw); return Unsafe.As<double, T>(ref v); }
        if (typeof(T) == typeof(DateTime)) { var v = Unsafe.As<ulong, DateTime>(ref raw); return Unsafe.As<DateTime, T>(ref v); }
        if (typeof(T) == typeof(TimeSpan)) { var v = Unsafe.As<ulong, TimeSpan>(ref raw); return Unsafe.As<TimeSpan, T>(ref v); }

        throw new NotSupportedException($"{typeof(T)} is not a 64-bit row buffer type.");
    }

    public T? Read128Bit<T>(int index)
        where T : struct
    {
        if (IsNull128(index))
            return null;

        var raw = GetBits128(index);

        if (typeof(T) == typeof(decimal)) { var v = Unsafe.As<Int128, decimal>(ref raw); return Unsafe.As<decimal, T>(ref v); }
        if (typeof(T) == typeof(Guid)) { var v = Unsafe.As<Int128, Guid>(ref raw); return Unsafe.As<Guid, T>(ref v); }

        throw new NotSupportedException($"{typeof(T)} is not a 128-bit row buffer type.");
    }

    // Reads a column boxed to object -- used only at the boundaries that still deal in
    // object (the output reader, sort/group comparisons, hash keys). `type` is the
    // column's non-nullable CLR type, used to pick the decode.
    public object? GetBoxedValue(RowBufferColumn column, Type type)
    {
        switch (column.Kind)
        {
            case RowBufferColumnKind.Object:
                return GetObject(column.Index);
            case RowBufferColumnKind.Bits32:
                return Box32(column.Index, type);
            case RowBufferColumnKind.Bits64:
                return Box64(column.Index, type);
            case RowBufferColumnKind.Bits128:
                return Box128(column.Index, type);
            default:
                throw ExceptionBuilder.UnexpectedValue(column.Kind);
        }
    }

    private object? Box32(int index, Type type)
    {
        if (type == typeof(int)) return Read32Bit<int>(index);
        if (type == typeof(uint)) return Read32Bit<uint>(index);
        if (type == typeof(short)) return Read32Bit<short>(index);
        if (type == typeof(ushort)) return Read32Bit<ushort>(index);
        if (type == typeof(byte)) return Read32Bit<byte>(index);
        if (type == typeof(sbyte)) return Read32Bit<sbyte>(index);
        if (type == typeof(char)) return Read32Bit<char>(index);
        if (type == typeof(bool)) return Read32Bit<bool>(index);
        if (type == typeof(float)) return Read32Bit<float>(index);
        throw new NotSupportedException($"{type} is not a 32-bit row buffer type.");
    }

    private object? Box64(int index, Type type)
    {
        if (type == typeof(long)) return Read64Bit<long>(index);
        if (type == typeof(ulong)) return Read64Bit<ulong>(index);
        if (type == typeof(double)) return Read64Bit<double>(index);
        if (type == typeof(DateTime)) return Read64Bit<DateTime>(index);
        if (type == typeof(TimeSpan)) return Read64Bit<TimeSpan>(index);
        throw new NotSupportedException($"{type} is not a 64-bit row buffer type.");
    }

    private object? Box128(int index, Type type)
    {
        if (type == typeof(decimal)) return Read128Bit<decimal>(index);
        if (type == typeof(Guid)) return Read128Bit<Guid>(index);
        throw new NotSupportedException($"{type} is not a 128-bit row buffer type.");
    }

}
