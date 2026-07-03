using System.Collections.Immutable;

namespace NQuery.CodeAnalysis.Iterators;

// The recursive-CTE driver (working-table model, breadth-first): emits the anchor's
// rows while staging them as the round-0 frontier, then ping-pongs two columnar
// stores -- each round re-opens the recursive members over the frontier (their
// RecursiveReferenceIterator leaves scan it through the shared RecursiveWorkTable),
// appends every produced row to the other store and emits it, then swaps and starts
// the next round. Recursion ends when a round produces no rows; a row produced
// beyond the maximum recursion level (100, matching SQL Server's MAXRECURSION
// default and the legacy engine) is an error.
//
// The inputs arrive pre-projected into the unified output order (one
// ProjectedRowBuffer per input, like the concatenation's), so appending to a store
// and exposing a stored row are both positional. The exposed buffer is an
// IndirectedRowBuffer pointed at a cursor over whichever store the current row was
// just appended to.
internal sealed class RecursionIterator : Iterator
{
    private const int MaxRecursionLevel = 100;

    private readonly Iterator _anchor;
    private readonly RowBuffer _anchorInput;
    private readonly ImmutableArray<Iterator> _members;
    private readonly ImmutableArray<RowBuffer> _memberInputs;
    private readonly RecursiveWorkTable _workTable;

    private readonly SpooledRowStore _storeA;
    private readonly SpooledRowStore _storeB;
    private readonly SpooledRowStore.Cursor _cursorA;
    private readonly SpooledRowStore.Cursor _cursorB;
    private readonly IndirectedRowBuffer _rowBuffer;

    private SpooledRowStore _frontier;
    private SpooledRowStore _produced;
    private bool _anchorDone;
    private int _memberIndex;
    private bool _memberIsOpen;
    private int _level;

    public RecursionIterator(Iterator anchor, RowBuffer anchorInput, ImmutableArray<Iterator> members, ImmutableArray<RowBuffer> memberInputs, RecursiveWorkTable workTable)
    {
        ThrowIfNull(anchor);
        ThrowIfNull(anchorInput);
        ThrowIfNull(workTable);

        _anchor = anchor;
        _anchorInput = anchorInput;
        _members = members;
        _memberInputs = memberInputs;
        _workTable = workTable;

        // All inputs share the unified layout, so either projection can template the
        // stores and the exposed buffer.
        _storeA = new SpooledRowStore(anchorInput);
        _storeB = new SpooledRowStore(anchorInput);
        _cursorA = _storeA.CreateCursor();
        _cursorB = _storeB.CreateCursor();
        _rowBuffer = new IndirectedRowBuffer(anchorInput, _cursorA);

        _frontier = _storeB;
        _produced = _storeA;
    }

    public override RowBuffer RowBuffer => _rowBuffer;

    public override void Open()
    {
        _anchor.Open();
        _anchorDone = false;

        _storeA.Clear();
        _storeB.Clear();
        _frontier = _storeB;
        _produced = _storeA;
        _workTable.Frontier = null;

        _memberIndex = 0;
        _memberIsOpen = false;
        _level = 0;
    }

    public override void Dispose()
    {
        _anchor.Dispose();
        foreach (var member in _members)
            member.Dispose();
    }

    public override bool Read()
    {
        if (!_anchorDone)
        {
            if (_anchor.Read())
            {
                Produce(_anchorInput);
                return true;
            }

            _anchorDone = true;
            if (!StartNextRound())
                return false;
        }

        while (true)
        {
            while (_memberIndex < _members.Length)
            {
                var member = _members[_memberIndex];

                if (!_memberIsOpen)
                {
                    member.Open();
                    _memberIsOpen = true;
                }

                if (member.Read())
                {
                    // The level is the row's recursion depth (anchor rows are level 0);
                    // matching the legacy engine, only a row actually produced beyond the
                    // limit is an error -- a frontier that yields nothing terminates fine.
                    if (_level > MaxRecursionLevel)
                        throw new InvalidOperationException(Resources.MaximumRecursionLevelExceeded);

                    Produce(_memberInputs[_memberIndex]);
                    return true;
                }

                _memberIndex++;
                _memberIsOpen = false;
            }

            if (!StartNextRound())
                return false;
        }
    }

    // Stages the row as part of the next frontier and exposes it as the current output row.
    private void Produce(RowBuffer source)
    {
        _produced.Append(source);

        var cursor = _produced == _storeA ? _cursorA : _cursorB;
        cursor.Row = _produced.Count - 1;
        _rowBuffer.ActiveRowBuffer = cursor;
    }

    // Promotes this round's produced rows to the frontier the references scan, and
    // recycles the old frontier as the next round's produced store. False when the
    // round produced nothing -- the fixpoint is reached.
    private bool StartNextRound()
    {
        if (_produced.Count == 0)
            return false;

        (_frontier, _produced) = (_produced, _frontier);
        _produced.Clear();
        _workTable.Frontier = _frontier;

        _level++;
        _memberIndex = 0;
        _memberIsOpen = false;
        return true;
    }
}
