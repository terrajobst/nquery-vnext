namespace NQuery.CodeAnalysis.Iterators;

// Like FilterIterator, but a row that fails the predicate is an error rather than
// something to skip. Its predicate takes the row buffer as a parameter, so the
// compiled predicate is shared across executions.
//
// When the assert is the correlated part of an Apply's right side, the outer buffer
// is prepended to the input buffer, matching the (outer ++ input) slot layout the
// predicate was compiled against.
internal sealed class AssertIterator : Iterator
{
    private readonly Iterator _input;
    private readonly CompiledPredicate _predicate;
    private readonly string _message;
    private readonly RowBuffer _predicateRowBuffer;

    public AssertIterator(Iterator input, CompiledPredicate predicate, string message, RowBuffer? outer)
    {
        ThrowIfNull(input);
        ThrowIfNull(predicate);
        ThrowIfNull(message);

        _input = input;
        _predicate = predicate;
        _message = message;
        _predicateRowBuffer = outer is null ? input.RowBuffer : new CombinedRowBuffer(outer, input.RowBuffer);
    }

    public override RowBuffer RowBuffer => _input.RowBuffer;

    public override void Open() => _input.Open();

    public override void Dispose() => _input.Dispose();

    public override bool Read()
    {
        if (!_input.Read())
            return false;

        if (!_predicate(_predicateRowBuffer))
            throw new InvalidOperationException(_message);

        return true;
    }
}
