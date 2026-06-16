namespace NQuery.CodeAnalysis.Iterators;

// A whole stream-aggregate compiled into three delegates that share one set of strongly-typed
// accumulator variables (hoisted into a closure -- a fresh set per factory call). There is no
// per-aggregate object and no boxed accumulator: each fold's state lives in its own CLR type.
//
//   Initialize()              -- seed every accumulator
//   Accumulate(rowBuffer)     -- fold the current row into every accumulator
//   StoreResults(outputArray) -- write every result after the grouping columns
internal sealed class EmittedAggregates
{
    public EmittedAggregates(Action initialize, Action<RowBuffer> accumulate, Action<object[]> storeResults)
    {
        Initialize = initialize;
        Accumulate = accumulate;
        StoreResults = storeResults;
    }

    public Action Initialize { get; }

    public Action<RowBuffer> Accumulate { get; }

    public Action<object[]> StoreResults { get; }
}
