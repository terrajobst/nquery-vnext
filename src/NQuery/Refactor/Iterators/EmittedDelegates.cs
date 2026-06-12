#nullable enable

namespace NQuery.Refactor.Iterators
{
    // Unlike the bound ExpressionBuilder -- which bakes the specific RowBuffer
    // instance into each delegate -- these delegates take the row buffer as a
    // parameter. That makes them independent of any one execution, so they can be
    // compiled once at emit time and reused across every CreateIterator() call.
    internal delegate object EmittedFunction(RowBuffer rowBuffer);

    internal delegate bool EmittedPredicate(RowBuffer rowBuffer);
}
