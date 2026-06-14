# Apply Pushdown

The **ApplyPushdown** pass (`NQuery.Refactor.Optimization.ApplyPushdown`) decorrelates `LogicalApply` operators by pushing them downward until the inner side no longer references outer slots. This converts correlated subqueries (produced by the Algebrizer) into efficient join-based plans.

**File**: `NQuery.Refactor.Optimization.ApplyPushdown`
**Instantiation**: Per-call (`new ApplyPushdown(type => ResolveComparer(dataContext, type))`)
**State**: Carries a `_comparerResolver` delegate for aggregate decorrelation

## Rewrite rules

### Base case

If `apply.OuterReferences.IsEmpty`, the apply becomes a plain `LogicalJoin` with no condition:

```
Before:            After:
Apply(L, R)        Join(L, R)
  ├── L              ├── L
  └── R              └── R
```

### Project push-through

For semi-applies, the inner project is dropped entirely. For inner/outer applies, the project is lifted above the join:

```
Before:
Apply(L, Project(R))
  ├── L
  └── Project(R)
        └── R

Semi apply → Apply(L, R)              Inner/outer apply → Project(Apply(L, R))
```

### Correlated filter

When an apply's right side is a filter referencing the left, the correlated predicates become join conditions:

```
Before:                              After:
Apply(L, Filter(R, pred(L,R)))       InnerJoin(L, R, pred(L,R))
  ├── L                                ├── L
  └── Filter(R, pred(L,R))             └── R
        └── R
```

### Correlated scalar aggregate

A `LeftOuter` apply over an aggregate with no `GROUP BY` requires special handling. The pass:

1. Constructs a **domain** by cloning L and grouping by correlation keys
2. Inner-joins the decorrelated body to the domain
3. Left-outer-joins the result back to L
4. Applies `COALESCE` to substitute `NULL` for empty groups (avoiding the "count bug")

```
Before:                              After:
LeftOuterApply(L, Agg(R))            Project(COALESCE(nvl, ...))
  ├── L                                └── LeftOuterJoin
  └── Agg(R)                                ├── L
                                            └── InnerJoin(Domain, Agg(R))
                                                    ├── Clone(L) grouped by keys
                                                    └── Agg(R)
```

## Why not all applies disappear

Apply operators that cannot be decorrelated remain as applies in the tree. These become **dependent joins** (nested loops with outer references) at the physical level. The pass is conservative — it only transforms when it can prove correctness.

## Dependencies

- Requires `DataContext` comparer resolvers for aggregate domain grouping
- Produces redundant projections that are cleaned up by [ProjectMerger](pass-project-merger.md) in the same batch
