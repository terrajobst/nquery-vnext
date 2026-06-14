namespace NQuery.Iterators;

internal sealed class LeftAntiSemiNestedLoopsIterator : NestedLoopsIterator
{
    private readonly Iterator _left;
    private readonly Iterator _right;
    private readonly EmittedPredicate _predicate;
    private readonly EmittedPredicate _passthruPredicate;
    private readonly RowBuffer _predicateRowBuffer;

    private bool _bof;
    private bool _advanceOuter;

    public LeftAntiSemiNestedLoopsIterator(Iterator left, Iterator right, EmittedPredicate predicate, EmittedPredicate passthruPredicate, RowBuffer? outer = null)
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
        while (true)
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

            if (_bof)
            {
                _bof = false;
                _advanceOuter = true;
                continue;
            }

            if (!_right.Read())
            {
                _advanceOuter = true;
                return true;
            }

            // Check predicate.
            var matchingRowFound = _predicate(_predicateRowBuffer);
            if (matchingRowFound)
                _advanceOuter = true;
        }
    }
}
