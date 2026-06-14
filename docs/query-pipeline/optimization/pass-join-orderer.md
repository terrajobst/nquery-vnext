# Join Orderer

The **JoinOrderer** pass (`NQuery.Refactor.Optimization.JoinOrderer`) normalizes and reorders inner-join regions for better execution. It turns inner join predicates into equi-join conditions and arranges joins into a left-deep tree using structural heuristics.

**File**: `NQuery.Refactor.Optimization.JoinOrderer`
**Instantiation**: Singleton (`JoinOrderer.Instance`)
**State**: Stateless
**Idempotent**: No — rebuilds join regions unconditionally (must run in a `Once` batch)

## Rewrite rules

### Join region detection

A **join region** is a connected subgraph of inner joins (including cross products). The pass collapses regions into their leaf inputs plus a pool of predicates:

```
Original bushy tree:                Region abstraction:        Left-deep result:

    InnerJoin                       Leaves: T1, T2, T3         InnerJoin
    ├── InnerJoin                   Predicates:                ├── T1
    │     ├── T1                    p12(T1,T2)                 └── InnerJoin(..., p12 ∧ p123)
    │     └── T2                    p123(T1,T2,T3)                   ├── T2
    └── T3                                                           └── T3
```

### Predicate attachment

Each predicate is attached to the lowest join whose inputs cover all its referenced slots:

```
Before:                                After:

Predicate p(A, B, C)                   Join(A, B, C)       ← p attached here
  ├── A                                  ├── Join(A, B)    ← does not cover C
  ├── B                                  │     ├── A
  └── C                                  │     └── B
                                         └── C
```

### Heuristic pick

`PickNext` selects the next input to join using a heuristic: prefer an input connected by a predicate to the already-joined set. This avoids unnecessary cartesian products:

```
Start with first leaf
        │
        ▼
 ┌─────►PickNext: prefer input with predicate edge to current set
 │        │
 │        ▼
 │      All leaves joined?
 │      │         │
 │      │ No      │ Yes
 │      │         │
 │      │         ▼
 │      │       Done
 └─────┘
```

```text
Available: {A, B, C, D}
Predicates: p(A,B), p(B,C), p(D)

Step 1: Pick A (seed)
Step 2: Pick B (connected by p(A,B) ✓)
Step 3: Pick C (connected by p(B,C) ✓)
Step 4: Pick D (no connection → cartesian, unavoidable)
```

### Outer join pull-up

Outer joins whose null-supplied side has no predicate referencing it are **pulled up** from the region. The preserved side is reordered within the region, and the outer join is re-applied after assembly:

```
Before pull-up:                     After pull-up:

InnerJoin(OuterJoin, T3)            LeftOuterJoin(InnerJoin(T1, T3), T2)
  ├── LeftOuterJoin(T1, T2)          ├── InnerJoin(T1, T3)
  │     ├── T1                       │     ├── T1
  │     └── T2                       │     └── T3
  └── T3                             └── T2

  └── T2 has no region predicates → pulled up
```

## Non-idempotence

The pass unconditionally rebuilds join regions. Even if the original tree is already left-deep, `ReferenceEquals` will differ because new operator instances are created. This is why the pass must run in a `Once` batch rather than `FixedPoint`.
