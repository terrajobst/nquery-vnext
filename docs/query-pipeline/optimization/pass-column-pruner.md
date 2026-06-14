# Column Pruner

The **ColumnPruner** pass (`NQuery.Refactor.Optimization.ColumnPruner`) removes value slots that are never referenced by any operator above the defining operator. This ensures the narrowest possible rows flow through sort, aggregate, and join operations.

**File**: `NQuery.Refactor.Optimization.ColumnPruner`
**Instantiation**: Singleton (`ColumnPruner.Instance`)
**State**: Per-invocation `Worker` inner class carries a `HashSet<ValueSlot> _used`

## Rewrite rules

### Liveness analysis

Unlike the base `LogicalOperatorRewriter` (which is post-order), the pruner performs a **pre-order** traversal. Liveness is computed top-down:

1. **Seed** `_used` with the root operator's output slots
2. **Walk pre-order**: before visiting children, record which slots the current operator consumes
3. **Prune at producers**: when reaching a definition site (table scan, compute, project, aggregate, union), remove any defined slot not in `_used`

```
Root                             _used = {x, y}       (seeded from root outputs)
  └── Filter uses: [x, z]        _used += {x, z}      (record consumption)
        └── Project(a, b, c)     _used += {a, b}      (project consumes a, b from child)
              outputs: [x, y, z]
              uses: [a, b]
                    └── TableScan(T) defines: [a, b, c]
                        _used at this point = {a, b}
                        → c is not in _used → pruned
                        Result: TableScan(T): [a, b]
```

### Per-operator pruning

| Operator | Pruning behavior |
|---|---|
| `TableScan` | Keeps at least one column (bare `COUNT(*)` needs rows). Unreferenced columns are dropped. |
| `ComputeScalar` | Drops computed slots that nothing consumes. If all computed slots are unused, the operator is removed entirely. |
| `Project` | Drops unused output slots. Passes through only referenced columns. |
| `Aggregate` | Drops unused aggregates. Group-by columns are always kept (needed for grouping). |
| `Union` | Narrows columns for `UNION ALL`. `DISTINCT UNION` keeps all columns (equality comparison requires full rows). |
| `Sort` | Marks sort-key slots as used. |
| `Top` | Marks sort-key and tie-breaker slots as used. |
| `Assert` | Marks condition slots as used. |

### Full example

```
Before pruning:                               After pruning:

Sort(a)                                       Sort(a)
  └── Project(a, b, c)                          └── Project(a)
        └── ComputeScalar(c = x + y)                 └── TableScan(T): a
              └── TableScan(T): a, b, d, e

  └── c, b, d, e unused → removed
```

## Why it runs last

Column pruning must run after all other passes because:

1. **Predicate pushdown** (SelectionPushdown) may add new filters that reference additional columns
2. **Join reordering** (JoinOrderer) may rearrange joins and expose new column references
3. **Outer join removal** (OuterJoinRemover) may change join types and alter the used-column set

Running pruning earlier would produce a plan that may need to re-fetch pruned columns when later passes introduce new references.
