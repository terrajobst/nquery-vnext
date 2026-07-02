using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

// The runtime row buffer is column-typed and container-segregated, but the iterator
// tests describe rows as untyped object?[] data. These helpers bridge the two: column
// types are inferred from the data (the first non-null value in each column), and a
// boxed value can be read/written by flat column index against an inferred layout.
internal static class RowBufferTestSupport
{
    public static Type[] InferColumnTypes(object?[,] data)
    {
        var rowCount = data.GetLength(0);
        var columnCount = data.GetLength(1);
        var types = new Type[columnCount];

        for (var j = 0; j < columnCount; j++)
        {
            types[j] = typeof(object);
            for (var i = 0; i < rowCount; i++)
            {
                if (data[i, j] is { } value)
                {
                    types[j] = value.GetType();
                    break;
                }
            }
        }

        return types;
    }

    // Builds a leaf row buffer from values, typing each column by its value's runtime
    // type (object for null) and storing it in the matching container.
    public static ArrayRowBuffer Buffer(params object?[] values)
    {
        var types = values.Select(v => v?.GetType() ?? typeof(object)).ToArray();
        var layout = RowBufferLayout.Create(types);
        var buffer = new ArrayRowBuffer(layout);
        for (var i = 0; i < values.Length; i++)
            buffer.WriteBoxed(layout.Columns[i], types[i], values[i]);
        return buffer;
    }

    // Resolves a flat column index over a buffer (whose columns have the given types)
    // to a typed entry.
    public static RowBufferEntry Entry(RowBuffer buffer, Type[] sourceTypes, int columnIndex)
    {
        var layout = RowBufferLayout.Create(sourceTypes);
        return new RowBufferEntry(buffer, layout.Columns[columnIndex], sourceTypes[columnIndex]);
    }

    // An int-typed key entry at a flat column index of an all-int buffer (flat order ==
    // 32-bit container order). Convenience for the join-key tests.
    public static RowBufferEntry IntKey(RowBuffer buffer, int columnIndex)
    {
        return new RowBufferEntry(buffer, new RowBufferColumn(RowBufferColumnKind.Bits32, columnIndex), typeof(int));
    }

    public static Type[] InferColumnTypes(object?[] singleColumnRows)
    {
        var type = typeof(object);
        foreach (var value in singleColumnRows)
        {
            if (value is not null)
            {
                type = value.GetType();
                break;
            }
        }

        return new[] { type };
    }
}

// Reads a column by flat index off a row buffer in tests. The flat index is the column's
// ordinal; the helper resolves its container address from the buffer's per-container
// counts (tests use homogeneously typed columns, so a kind's flat order is its container
// order). Mirrors what the compiled expressions do, but spelled out for hand-written
// predicates.
internal static class RowBufferTestExtensions
{
    extension(RowBuffer rowBuffer)
    {
        public int Int32(int columnIndex)
        {
            return rowBuffer.Read32Bit<int>(columnIndex)!.Value;
        }

        public int? NullableInt32(int columnIndex)
        {
            return rowBuffer.Read32Bit<int>(columnIndex);
        }

        public string? String(int columnIndex)
        {
            return rowBuffer.ReadObject<string?>(columnIndex);
        }

        public bool Bool(int columnIndex)
        {
            return rowBuffer.Read32Bit<bool>(columnIndex)!.Value;
        }
    }
}
