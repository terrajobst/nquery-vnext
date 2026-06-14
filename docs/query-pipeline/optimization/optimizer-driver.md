# Optimizer Driver

The `LogicalOptimizer` class drives the optimization process by running a sequence of **batches** against the logical operator tree.

## Batch strategies

Two strategies control how a batch is executed:

### FixedPoint

A **FixedPoint** batch runs its passes repeatedly until the tree stabilizes (no pass produces a change). This is necessary for idempotent algebraic rewrites where one pass may enable further opportunities for the same or another pass.

```
┌──────────────────┐
│  Run all passes  │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Tree changed?   │──── Yes ────► (repeat, max 100)
└────────┬─────────┘
         │ No
         ▼
      Converged
```

```csharp
private const int MaxIterations = 100;

// FixedPoint iteration
for (var iteration = 0; iteration < MaxIterations; iteration++)
{
    var before = root;
    foreach (var pass in batch.Passes)
        root = pass.RewriteRelation(root);
    if (ReferenceEquals(root, before))
        return root; // converged
}

throw new InvalidOperationException(
    $"Logical optimization batch '{batch.Name}' did not converge within {MaxIterations} iterations; " +
    "a pass is likely not idempotent.");
```

### Once

A **Once** batch runs its passes exactly one time and moves on. Used for passes that are not idempotent (e.g., JoinOrderer rebuilds join regions unconditionally) or where a single application is sufficient (e.g., OuterJoinRemover, ColumnPruner).

```
┌──────────────────┐
│  Run all passes  │
└────────┬─────────┘
         │
         ▼
        Done
```

```csharp
// Once: run each pass exactly once
foreach (var pass in batch.Passes)
    root = pass.RewriteRelation(root);
return root;
```

## Why the chosen order

| Ordering rationale | Why |
|---|---|
| Decorrelation first | Converts correlated subqueries into plain joins so subsequent passes can reason about the full join graph |
| Outer join removal before join ordering | Freed inner joins participate in the join region for reordering |
| Join ordering before second selection pushdown | Placing predicates as join conditions exposes new single-table conjuncts to push to leaves |
| Column pruning last | All predicates and operators have settled; unused slots won't reappear |

## Identity short-circuit

The `LogicalOperatorRewriter` base class uses reference equality to detect changes:

```csharp
// If the recursively-rewritten child is the same object, return the original node.
protected virtual LogicalOperator RewriteFilter(LogicalFilter node)
{
    var input = RewriteRelation(node.Input);
    return input == node.Input ? node : new LogicalFilter(input, node.Conditions);
}
```

The driver relies on `ReferenceEquals(root, before)` after running all passes in a batch to determine whether to iterate again. This makes the identity check critical for convergence detection.

## Thread safety

Each call to `Optimize` constructs fresh batch instances. Per-call passes (`ApplyPushdown`, `OuterJoinRemover`) are created per optimization call. Singleton passes (`SelectionPushdown`, `JoinOrderer`, `ProjectMerger`, `ColumnPruner`) are stateless and safe. The optimizer is safe for concurrent use as long as different `DataContext` instances are used.

## Debugging and showplan

For debugging and execution plan visualization, the optimizer also exposes `GetOptimizationSteps`, a `yield return` variant that emits each intermediate tree along with the pass name that produced it, without the convergence assertion.
