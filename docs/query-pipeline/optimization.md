# Optimization

The **LogicalOptimizer** (`NQuery.Refactor.Optimization.LogicalOptimizer`) transforms the logical algebra tree into an equivalent but more efficient form. It applies a sequence of **optimization passes** grouped into **batches**.

## Driver

```csharp
public static LogicalQuery Optimize(LogicalQuery query, DataContext dataContext);
```

Passes are grouped into ordered batches. Each batch runs with a strategy:

- **FixedPoint**: The batch runs repeatedly until no pass produces a change (or up to 100 iterations).
- **Once**: The batch runs exactly one time.

## Batches and passes

```
Batch 1 — Decorrelation (FixedPoint)
  ├── Apply Pushdown
  └── Selection Pushdown

Batch 2 — Outer join removal (Once)
  └── Outer Join Remover

Batch 3 — Join ordering (Once)
  └── Join Orderer

Batch 4 — Selection (FixedPoint)
  └── Selection Pushdown

Batch 5 — Column pruning (Once)
  └── Column Pruner
```

All passes inherit from `LogicalOperatorRewriter`:

```
LogicalOperatorRewriter
├── ApplyPushdown
├── SelectionPushdown
├── OuterJoinRemover
├── JoinOrderer
├── ProjectMerger
└── ColumnPruner
```

Helper (static, not a rewriter): `NullRejection` — computes null-rejection sets for outer joins.

### Apply Pushdown

Pushes `Apply` operators downward in the tree, decorrelating them into ordinary joins when the inner side no longer references outer slots. This is the core decorrelation pass — it converts correlated subqueries (produced by the Algebrizer) into efficient join-based plans.

**File**: `NQuery.Refactor.Optimization.ApplyPushdown`

The pass requires access to comparers from the `DataContext` for aggregate decorrelation. It is constructed per-optimization call with a `resolveComparer` delegate.

### Selection Pushdown

Pushes `LogicalFilter` predicates as close to their source tables as possible. Single-table conjuncts are pushed through joins, applies, projections, and other operators. This reduces the number of rows flowing through the pipeline early.

**File**: `NQuery.Refactor.Optimization.SelectionPushdown`

A stateless singleton (`SelectionPushdown.Instance`) since it has no per-tree state.

### Outer Join Remover

Tightens `LEFT/RIGHT OUTER JOIN` operators into `INNER JOIN` when a predicate above the join rejects nulls on the null-supplied side. For example, `LEFT JOIN ... WHERE right.Column IS NOT NULL` can become `INNER JOIN`. This must run before join ordering so freed inner joins can participate in join reordering.

**File**: `NQuery.Refactor.Optimization.OuterJoinRemover`

Accumulates per-tree state (tracking rejected-null sides), so a fresh instance is created per optimization call.

### Join Orderer

Reorders inner-join regions for better execution. It turns inner join predicates into equi-join conditions and arranges joins into a left-deep tree. Currently uses structural heuristics rather than cost-based decisions.

**File**: `NQuery.Refactor.Optimization.JoinOrderer`

A stateless singleton (`JoinOrderer.Instance`). Not idempotent — it rebuilds join regions unconditionally.

### Column Pruner

Removes value slots that are never referenced by any operator above the defining operator. This ensures the narrowest possible rows flow into sort, aggregate, and join operations.

**File**: `NQuery.Refactor.Optimization.ColumnPruner`

A stateless singleton (`ColumnPruner.Instance`). Runs last, after all predicates have settled.

## Fixed-point batching

The batching strategy groups idempotent algebraic rewrites (decorrelation, selection pushdown) into fixed-point batches that converge to a normal form. Non-idempotent passes (join ordering, column pruning) run once. Selection pushdown runs again after join ordering because placing predicates as join conditions exposes new single-table conjuncts to push to the leaves.

```csharp
internal static class LogicalOptimizer
{
    private const int MaxIterations = 100;

    public static LogicalQuery Optimize(LogicalQuery query, DataContext dataContext);
    public static LogicalOperator Optimize(LogicalOperator root, DataContext dataContext);
}
```
