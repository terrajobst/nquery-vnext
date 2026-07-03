namespace NQuery.CodeAnalysis.Iterators;

// A whole stream-aggregate compiled into three delegates that share one set of strongly-typed
// accumulator variables (hoisted into a closure -- a fresh set per factory call). There is no
// per-aggregate object and no boxed accumulator: each fold's state lives in its own CLR type.
//
//   Initialize()                -- seed every accumulator
//   Accumulate(rowBuffer)       -- fold the current row into every accumulator
//   StoreResults(outputBuffer)  -- write every result, typed, into the aggregate columns
internal sealed class CompiledAggregates
{
    public CompiledAggregates(Action initialize, Action<RowBuffer> accumulate, Action<ArrayRowBuffer> storeResults)
    {
        ThrowIfNull(initialize);
        ThrowIfNull(accumulate);
        ThrowIfNull(storeResults);

        Initialize = initialize;
        Accumulate = accumulate;
        StoreResults = storeResults;
    }

    public Action Initialize { get; }

    public Action<RowBuffer> Accumulate { get; }

    public Action<ArrayRowBuffer> StoreResults { get; }
}
