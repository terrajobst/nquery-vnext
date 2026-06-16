# Optimization

The **LogicalOptimizer** (`NQuery.Refactor.Optimization.LogicalOptimizer`) transforms the logical algebra tree into an equivalent but more efficient form. It applies a sequence of **optimization passes** grouped into **batches**.

```csharp
public static LogicalQuery Optimize(LogicalQuery query, Catalog catalog);
```

## Pipeline position

The optimizer sits between algebrization and planning, with a feedback loop into itself:

```
    Algebrization ──► Optimization ──► Planner
                          ▲  │
                          └──┘ (fixed-point iterations)
```

## Batch sequence

The optimizer runs five batches in order. Each batch contains one or more passes:

```
  Batch 1: Decorrelation (FixedPoint)
       │
       ▼
  Batch 2: Outer join removal (Once)
       │
       ▼
  Batch 3: Join ordering (Once)
       │
       ▼
  Batch 4: Selection (FixedPoint)
       │
       ▼
  Batch 5: Column pruning (Once)
```

| Batch | Strategy | Passes | Purpose |
|-------|----------|--------|---------|
| 1 — Decorrelation | FixedPoint | ApplyPushdown, SelectionPushdown, ProjectMerger | Convert correlated subqueries to joins |
| 2 — Outer join removal | Once | OuterJoinRemover | Tighten outer joins to inner where possible |
| 3 — Join ordering | Once | JoinOrderer | Reorder inner-join regions into left-deep trees |
| 4 — Selection | FixedPoint | SelectionPushdown | Push predicates down after join reordering |
| 5 — Column pruning | Once | ColumnPruner | Remove unreferenced value slots |

## Pass hierarchy

All passes inherit from `LogicalOperatorRewriter`, a bottom-up tree rewriter with identity short-circuit:

```
LogicalOperatorRewriter
├── ApplyPushdown
├── SelectionPushdown
├── OuterJoinRemover
├── JoinOrderer
├── ProjectMerger
└── ColumnPruner
```

`LogicalOperatorRewriter` provides virtual `RewriteXxx` methods for each operator kind (`RewriteFilter`, `RewriteJoin`, etc.). Each method recurses into children and returns a new node only if a child changed — otherwise returns the original node by reference. The driver uses reference equality to detect convergence.

A separate static helper, `NullRejection.IsRejectingNull`, is used by `OuterJoinRemover` to determine which expressions guarantee null rejection.

## Pass reference

| Pass | Instantiation | State | File |
|------|---------------|-------|------|
| [ApplyPushdown](pass-apply-pushdown.md) | Per-call (`new`) | Comparer resolver delegate | `NQuery.Refactor.Optimization.ApplyPushdown` |
| [SelectionPushdown](pass-selection-pushdown.md) | Singleton (`Instance`) | Stateless | `NQuery.Refactor.Optimization.SelectionPushdown` |
| [OuterJoinRemover](pass-outer-join-remover.md) | Per-call (`new`) | Null-rejected slot set | `NQuery.Refactor.Optimization.OuterJoinRemover` |
| [JoinOrderer](pass-join-orderer.md) | Singleton (`Instance`) | Stateless | `NQuery.Refactor.Optimization.JoinOrderer` |
| [ProjectMerger](pass-project-merger.md) | Singleton (`Instance`) | Stateless | `NQuery.Refactor.Optimization.ProjectMerger` |
| [ColumnPruner](pass-column-pruner.md) | Singleton (`Instance`) | Per-call worker | `NQuery.Refactor.Optimization.ColumnPruner` |
| [NullRejection](pass-outer-join-remover.md#nullrejection-helper) | Static helper | Pure function | `NQuery.Refactor.Optimization.NullRejection` |

See the [Optimizer Driver](optimizer-driver.md) for details on batching and iteration semantics.
