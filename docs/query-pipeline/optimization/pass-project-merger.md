# Project Merger

The **ProjectMerger** pass (`NQuery.Refactor.Optimization.ProjectMerger`) collapses redundant projections in the logical tree. It removes adjacent projections and identity projections that appear after decorrelation transforms.

**File**: `NQuery.Refactor.Optimization.ProjectMerger`
**Instantiation**: Singleton (`ProjectMerger.Instance`)
**State**: Stateless

## Rewrite rules

### Adjacent project collapse

When one `LogicalProject` sits on top of another, the inner project is redundant because the outer project computes its outputs from the grandchild's *defined* slots (not the inner project's output slots):

```
Before:                                After:

Project(y = a + b)                     Project(y = a + b)
  └── Project(a, b, c)                    └── TableScan(T)
        └── TableScan(T)
```

| Input | Output |
|---|---|
| `Proj_y(Proj_x(input))` | `Proj_y(input)` |

### Identity project removal

A projection whose output slots match its input's output slots (same slots, same order) is a no-op and is removed:

```
Before:                                After:

Project(a, b)                          TableScan(T): a, b, c
  └── TableScan(T): a, b, c
```

## Why this pass exists

[ApplyPushdown](pass-apply-pushdown.md) is the primary source of redundant projections. When the decorrelation pass pushes an apply through a project, it wraps the decorrelated result in a new project layer. This creates adjacent-project patterns that ProjectMerger cleans up.

The [ColumnPruner](pass-column-pruner.md) pass narrows a project's columns but never removes the operator itself. Without ProjectMerger, identity projects would accumulate in the tree.

```
ApplyPushdown ──► produces nested Project(Project(...)) ──┐
                                                           │
ColumnPruner  ──► narrows output but keeps operator ──────┤
                                                           │
                                                           ▼
                                                    ProjectMerger
                                                           │
                                                           ▼
                                                      Clean tree
                                                      (single Project)
```

## Iterator elimination

At the physical level, a projection becomes a pass-through operator when selected columns align with the input. ProjectMerger eliminates the operator entirely, saving both compile-time and (potentially) runtime overhead.
