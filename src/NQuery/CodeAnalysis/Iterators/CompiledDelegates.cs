namespace NQuery.CodeAnalysis.Iterators;

// These delegates take the row buffer as a parameter rather than baking a specific
// RowBuffer instance into each delegate. That makes them independent of any one
// execution, so they can be compiled once at emit time and reused across every
// CreateIterator() call.
internal delegate object CompiledFunction(RowBuffer rowBuffer);

internal delegate bool CompiledPredicate(RowBuffer rowBuffer);
