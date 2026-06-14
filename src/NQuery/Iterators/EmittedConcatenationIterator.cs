using System.Collections.Immutable;

namespace NQuery.Iterators;

// Concatenation (UNION ALL): exhausts each input in turn, exposing its rows through
// a stable output buffer. Each input maps its own columns to the unified output
// order via a ProjectedRowBuffer; an IndirectedRowBuffer swaps to the active input's
// projection as we advance from one input to the next.
//
// Distinctness (plain UNION) is handled by a distinct sort the planner places above
// this node, so the concatenation itself never deduplicates.
internal sealed class EmittedConcatenationIterator : Iterator
{
    private readonly ImmutableArray<Iterator> _inputs;
    private readonly ImmutableArray<ProjectedRowBuffer> _inputRowBuffers;
    private readonly IndirectedRowBuffer _rowBuffer;

    private int _currentInputIndex;
    private bool _currentInputIsOpen;

    public EmittedConcatenationIterator(IEnumerable<Iterator> inputs, IEnumerable<ImmutableArray<RowBufferEntry>> entries)
    {
        _inputs = inputs.ToImmutableArray();
        _inputRowBuffers = entries.Select(e => new ProjectedRowBuffer(e)).ToImmutableArray();
        _rowBuffer = new IndirectedRowBuffer(_inputRowBuffers[0].Count, _inputRowBuffers[0]);
    }

    public override RowBuffer RowBuffer => _rowBuffer;

    public override void Open()
    {
        _currentInputIndex = 0;
        _currentInputIsOpen = false;
    }

    public override void Dispose()
    {
        foreach (var iterator in _inputs)
            iterator.Dispose();
    }

    public override bool Read()
    {
        while (_currentInputIndex < _inputs.Length)
        {
            var currentInput = _inputs[_currentInputIndex];

            if (!_currentInputIsOpen)
            {
                currentInput.Open();
                _rowBuffer.ActiveRowBuffer = _inputRowBuffers[_currentInputIndex];
                _currentInputIsOpen = true;
            }

            if (currentInput.Read())
                return true;

            _currentInputIndex++;
            _currentInputIsOpen = false;
        }

        return false;
    }
}
