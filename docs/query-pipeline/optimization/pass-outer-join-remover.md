# Outer Join Remover

The **OuterJoinRemover** pass (`NQuery.Refactor.Optimization.OuterJoinRemover`) tightens `LEFT/RIGHT/FULL OUTER JOIN` operators into `INNER JOIN` when predicates above the join reject nulls on the null-supplied side.

**File**: `NQuery.Refactor.Optimization.OuterJoinRemover`
**Instantiation**: Per-call (`new OuterJoinRemover()`)
**State**: `HashSet<ValueSlot> _nullRejected` — tracks null-rejected slots as the pass descends the tree

## Rewrite rules

### Null-rejection tightening

```
Before:                                 After:
Filter(right.col IS NOT NULL)           InnerJoin(L, R, pred)
  └── LeftOuterJoin(L, R, pred)           ├── L
        ├── L                             └── R
        └── R

  └── right.col is null-rejected at the filter
```

| Original join | Null-rejected side | Result |
|---|---|---|
| `LEFT OUTER JOIN` | Right | `INNER JOIN` |
| `FULL OUTER JOIN` | Both | `INNER JOIN` |
| `FULL OUTER JOIN` | Left only | `LEFT OUTER JOIN` (left-preserving) |
| `FULL OUTER JOIN` | Right only | `LEFT OUTER JOIN` (right-preserving, normalized) |
| `RIGHT OUTER JOIN` | — | Normalized to `LEFT OUTER JOIN` before this pass |

The pass walks the tree top-down. When it encounters a `LogicalFilter` or an inner/semi join condition, it uses `NullRejection.IsRejectingNull` to determine which slots are guaranteed non-null:

```
Filter(x IS NOT NULL)
  │
  └── rejects null on x ──► propagates down to subtree
                              │
                              └── LeftOuterJoin(L, R)
                                    │
                                    └── x from right side? → InnerJoin
```

| Context | Contribution to `_nullRejected` |
|---|---|
| Filter conjuncts | All slots null-rejected by the conjunction |
| Inner/Semi join condition | Both sides contribute |
| Left outer join condition | Only the right side (preserved side) contributes |
| Full outer join condition | Neither side contributes |

### Constant relation removal

A single-row, no-column constant relation collapses to the other side of an inner join, with the join condition promoted to a filter:

```
Before:                              After:
InnerJoin(L, Constant(no cols))      Filter(L, condition)
  ├── L                                └── L
  └── Constant
```

## Why it must run before join ordering

Once an outer join is tightened to an inner join, the freed inner join can participate in the join region during [JoinOrderer](pass-join-orderer.md) reordering. Running this pass late would miss reordering opportunities.

## NullRejection helper

`NullRejection.IsRejectingNull(LogicalExpression, ValueSlot)` is a static helper that conservatively checks whether an expression is guaranteed to evaluate to `FALSE` or `UNKNOWN` when a given slot is `NULL`:

| Expression type | Behavior |
|---|---|
| `ValueSlot` (same slot) | `true` |
| `IsNull` | `true` when the argument is the slot (flipped) |
| `Unary` (NOT) | Reverses the inner result |
| `Binary` (AND) | Both sides must reject |
| `Binary` (OR, other) | Either side rejects |
| `PropertyAccess` target | Rejects if target slot is null |
| `MethodInvocation` target | Rejects if target slot is null (arguments not checked) |
| Literals, CASE, function calls | `false` (conservative) |
