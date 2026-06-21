namespace NQuery.CodeAnalysis.Iterators;

internal sealed class LeftSemiNestedLoopsIterator : NestedLoopsIterator
{
    private readonly Iterator _left;
    private readonly Iterator _right;
    private readonly CompiledPredicate _predicate;
    private readonly CompiledPredicate _passthruPredicate;
    private readonly RowBuffer _predicateRowBuffer;

    private bool _bof;
    private bool _advanceOuter;

    public LeftSemiNestedLoopsIterator(Iterator left, Iterator right, CompiledPredicate predicate, CompiledPredicate passthruPredicate, RowBuffer? outer = null)
    {
        _left = left;
        _right = right;
        _predicate = predicate;
        _passthruPredicate = passthruPredicate;

        // When correlated (inside an Apply), the outer buffer is prepended to the
        // (left ++ right) buffer, matching the (outer ++ left ++ right) layout the
        // predicate was compiled against.
        var combined = new CombinedRowBuffer(left.RowBuffer, right.RowBuffer);
        _predicateRowBuffer = outer is null ? combined : new CombinedRowBuffer(outer, combined);
    }

    public override RowBuffer RowBuffer
    {
        get { return _left.RowBuffer; }
    }

    public override void Open()
    {
        _left.Open();
        _advanceOuter = false;
        _bof = true;
    }

    public override void Dispose()
    {
        _left.Dispose();
        _right.Dispose();
    }

    public override bool Read()
    {
        var matchingRowFound = false;
        while (!matchingRowFound)
        {
            if (_advanceOuter)
            {
                _advanceOuter = false;

                if (!_left.Read())
                    return false;

                if (_passthruPredicate(_predicateRowBuffer))
                {
                    _advanceOuter = true;
                    return true;
                }

                _right.Open();
            }

            // If we are bof or the inner is eof, reset the inner and
            // advance both cursors.
            if (_bof || !_right.Read())
            {
                _bof = false;
                _advanceOuter = true;
                continue;
            }

            // Check predicate.
            matchingRowFound = _predicate(_predicateRowBuffer);

            if (matchingRowFound)
                _advanceOuter = true;
        }

        return true;
    }
}
