# Selection Pushdown

The **SelectionPushdown** pass (`NQuery.Refactor.Optimization.SelectionPushdown`) pushes `LogicalFilter` predicates as close to their source tables as possible. Single-table conjuncts are pushed through joins, applies, projections, and other operators to reduce row cardinality early.

**File**: `NQuery.Refactor.Optimization.SelectionPushdown`
**Instantiation**: Singleton (`SelectionPushdown.Instance`)
**State**: Stateless

## Rewrite rules

### Filter-through-join pushdown

When a filter sits above an inner join, the pass splits the filter's conjuncts into three groups:

- **Left-only**: pushed below the join on the left side
- **Right-only**: pushed below the join on the right side
- **Both-sides**: remain above the join

```
Before:                              After:
Filter(σp ∧ q(L) ∧ r(R))            InnerJoin(Filter(L, q), Filter(R, r), p)
  └── InnerJoin(L, R, p)              ├── Filter(L, q)
        ├── L                         │     └── L
        └── R                         └── Filter(R, r)
                                            └── R
```

### Conjunct routing

| Conjunct references | Pushed to |
|---|---|
| Only left input slots | Left side (new filter or merged into existing left filter) |
| Only right input slots | Right side (new filter or merged into existing right filter) |
| Both sides | Stays above the join |
| Neither (constant) | Pushed to either side |

```
Filter(conds) above InnerJoin(L,R)
        │
        ├── references only L ──► Push below on L
        ├── references only R ──► Push below on R
        ├── references both  ──► Stay above join
        └── references neither─► Push to L or R
```

## Outer join safety

Predicates are **not** pushed through outer joins (preserving side would change `LEFT/RIGHT/FULL` semantics into inner-join semantics). The pass restricts itself to inner joins only.

## Batch placement

SelectionPushdown runs in **two** batches:

1. **Batch 1 (Decorrelation, FixedPoint)** — After decorrelation, pushes predicates that were correlated filter conditions down to tables
2. **Batch 4 (Selection, FixedPoint)** — After join ordering, predicates placed as join conditions may expose new single-table conjuncts that can be pushed further down
