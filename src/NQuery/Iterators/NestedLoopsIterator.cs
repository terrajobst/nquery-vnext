namespace NQuery.Iterators;

// Marker base for the nested-loops join family. Unlike the bound iterators, the
// predicate and passthru delegates take the (combined) row buffer as a parameter,
// so they are compiled once at emit time and shared across executions.
internal abstract class NestedLoopsIterator : Iterator
{
}
