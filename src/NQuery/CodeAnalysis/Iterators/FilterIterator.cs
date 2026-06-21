namespace NQuery.CodeAnalysis.Iterators;

// Skips input rows whose predicate yields false. The predicate takes the row
// buffer as a parameter, so the compiled predicate is shared across executions
// rather than bound to one.
//
// When this filter is the correlated part of an Apply's right side, its predicate
// references the outer (left) row. The outer buffer is then prepended to the input
// buffer, matching the (outer ++ input) slot layout the predicate was compiled
// against; the rows this iterator exposes are still just its input's.
internal sealed class FilterIterator : Iterator
{
    private readonly Iterator _input;
    private readonly CompiledPredicate _predicate;
    private readonly RowBuffer _predicateRowBuffer;

    public FilterIterator(Iterator input, CompiledPredicate predicate, RowBuffer? outer)
    {
        _input = input;
        _predicate = predicate;
        _predicateRowBuffer = outer is null ? input.RowBuffer : new CombinedRowBuffer(outer, input.RowBuffer);
    }

    public override RowBuffer RowBuffer => _input.RowBuffer;

    public override void Open() => _input.Open();

    public override void Dispose() => _input.Dispose();

    public override bool Read()
    {
        while (_input.Read())
        {
            if (_predicate(_predicateRowBuffer))
                return true;
        }

        return false;
    }
}
