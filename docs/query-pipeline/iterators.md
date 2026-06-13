# Iterators

Iterators (`NQuery.Refactor.Iterators`) are the runtime execution engines that produce result rows. Each iterator implements the standard pull-based model:

```csharp
internal abstract class Iterator
{
    public abstract bool GetNext();
    public RowBuffer RowBuffer { get; }
}
```

- `GetNext()` advances to the next row and returns `true`, or returns `false` when no more rows are available.
- `RowBuffer` provides access to the current row's slot values by index.

## Type Hierarchy

All iterators inherit from `Iterator`:

```
Iterator
├── NestedLoopsIterator
│   ├── InnerNestedLoopsIterator              — left rows with matching right rows
│   ├── LeftOuterNestedLoopsIterator          — preserves left with null right
│   ├── LeftSemiNestedLoopsIterator           — left rows with any match
│   ├── ProbingLeftSemiNestedLoopsIterator    — semi with probe filter
│   └── LeftAntiSemiNestedLoopsIterator       — left rows with no match
├── SortIterator
│   └── DistinctSortIterator                  — sort + deduplicate
├── TopIterator
│   └── TopWithTiesIterator                   — top including ties
├── ConstantIterator                          — one row
├── EmptyIterator                             — zero rows
├── EmittedAssertIterator                     — validates condition
├── EmittedComputeScalarIterator              — evaluates computed slots
├── EmittedConcatenationIterator              — concatenates child rows
├── EmittedFilterIterator                     — evaluates predicate
├── EmittedHashMatchIterator                  — hash equi-join
├── EmittedStreamAggregateIterator            — sorted stream aggregation
├── ProjectionIterator                        — passes subset of values
└── TableIterator                             — reads from ITable
```

## Iterator construction

`CreateIterator()` on `ExecutablePlan` walks the executable operator tree and creates the corresponding iterator for each node, wiring child iterators and compiled delegates together. The row buffer layout is determined by the slot definitions known at emit time.
