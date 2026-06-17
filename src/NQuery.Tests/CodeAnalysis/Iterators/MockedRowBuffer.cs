using NQuery.CodeAnalysis.Iterators;

namespace NQuery.Tests.CodeAnalysis.Iterators;

// A test row buffer backed by a real ArrayRowBuffer. Columns are typed (inferred from
// the test data) and stored in their container; values are set/read boxed through the
// same boundary helpers the engine uses.
internal sealed class MockedRowBuffer : RowBuffer
{
    private readonly Type[] _types;
    private readonly RowBufferLayout _layout;
    private readonly ArrayRowBuffer _buffer;

    public MockedRowBuffer(Type[] types)
    {
        _types = types;
        _layout = RowBufferLayout.Create(types);
        _buffer = new ArrayRowBuffer(_layout);
    }

    public MockedRowBuffer(object?[] values)
        : this(values.Select(v => v?.GetType() ?? typeof(object)).ToArray())
    {
        SetRow(values);
    }

    public void SetRow(object?[] values)
    {
        for (var i = 0; i < values.Length; i++)
            _buffer.WriteBoxed(_layout.Columns[i], _types[i], values[i]);
    }

    public override int ObjectCount => _buffer.ObjectCount;

    public override int Bits32Count => _buffer.Bits32Count;

    public override int Bits64Count => _buffer.Bits64Count;

    public override int Bits128Count => _buffer.Bits128Count;

    public override object? GetObject(int index) => _buffer.GetObject(index);

    public override uint GetBits32(int index) => _buffer.GetBits32(index);

    public override ulong GetBits64(int index) => _buffer.GetBits64(index);

    public override Bits128 GetBits128(int index) => _buffer.GetBits128(index);

    public override bool IsNull32(int index) => _buffer.IsNull32(index);

    public override bool IsNull64(int index) => _buffer.IsNull64(index);

    public override bool IsNull128(int index) => _buffer.IsNull128(index);
}
